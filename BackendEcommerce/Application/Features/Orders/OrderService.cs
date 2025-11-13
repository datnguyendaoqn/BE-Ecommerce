using BackendEcommerce.Application.Features.AddressBooks.Contracts;
using BackendEcommerce.Application.Features.Carts.Contracts;
using BackendEcommerce.Application.Features.Orders.Contracts;
using BackendEcommerce.Application.Features.Orders.DTOs;
using BackendEcommerce.Application.Features.Products.Contracts;
using BackendEcommerce.Application.Shared.DTOs;
using BackendEcommerce.Infrastructure.Persistence.Data;
using BackendEcommerce.Infrastructure.Persistence.Models;
using BackendEcommerce.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BackendEcommerce.Application.Features.Orders
{
    /// <summary>
    /// (ĐÃ VIẾT LẠI (REWRITTEN): Để hỗ trợ (support) "Tick" (Chọn) Item (Món hàng)
    /// và Ghi chú (Note) (Note) riêng (separate) cho Cửa hàng (Shop) (Shop))
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        // ... (Constructor (Hàm dựng) và các tiêm (inject) khác giữ nguyên)
        private readonly IOrderItemRepository _orderItemRepo;
        private readonly IShopRepository _shop;
        private readonly IProductVariantRepository _variantRepo;
        private readonly IAddressBookRepository _addressRepo;
        private readonly ICartService _cartService;
        private readonly EcomDbContext _context;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepo,
            IOrderItemRepository orderItemRepo,
            IProductVariantRepository variantRepo,
            IShopRepository shop,
            IAddressBookRepository addressRepo,
            ICartService cartService,
            EcomDbContext context,
            ILogger<OrderService> logger)
        {
            _orderRepo = orderRepo;
            _orderItemRepo = orderItemRepo;
            _variantRepo = variantRepo;
            _addressRepo = addressRepo;
            _shop = shop;
            _cartService = cartService;
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponseDTO<CreateOrderResponseDto>> CreateOrderAsync(int customerId, CreateOrderRequestDto dto)
        {
            // === 1. LẤY TOÀN BỘ GIỎ HÀNG (CART) (CART) (TỪ REDIS) ===
            var cart = await _cartService.GetCartSnapshotAsync(customerId);

            // === 2. LỌC (FILTER) (THEO "TICKED") (GIẢI QUYẾT LỖ HỔNG 1) ===
            // (Chỉ lấy các món hàng (item) mà FE (Frontend) (Giao diện) (Giao diện) gửi (send) (đã "tick" (ticked) (ticked)))
            var itemsToCheckout = cart.Items
                .Where(item => dto.TickedVariantIds.Contains(item.ProductVariantId))
                .ToList();

            if (!itemsToCheckout.Any())
            {
                return new ApiResponseDTO<CreateOrderResponseDto> { IsSuccess = false, Code = 400, Message = "No valid items selected for checkout." };
            }

            // === 3. CHECK (KIỂM TRA) ĐỊA CHỈ (ADDRESS) & PM (PHƯƠNG THỨC) (Giữ nguyên) ===
            var address = await _addressRepo.GetByIdAndUserIdAsync(dto.AddressBookId, customerId);
            if (address == null)
            {
                return new ApiResponseDTO<CreateOrderResponseDto> { IsSuccess = false, Code = 404, Message = "Address not found or permission denied." };
            }
            if (dto.PaymentMethod != "COD")
            {
                return new ApiResponseDTO<CreateOrderResponseDto> { IsSuccess = false, Code = 400, Message = "Only 'COD' is supported." };
            }

            // === 4. CHUẨN BỊ (PREPARE) GHI CHÚ (NOTE) (NOTE) (GIẢI QUYẾT LỖ HỔNG 2) ===
            // (Chuyển List (Danh sách) Ghi chú (Note) (Note) thành Dictionary (Từ điển) (Lookup (Tra cứu)) (Tra cứu) (Lookup (Tra cứu)) (Tra cứu) (O(1)) (nhanh))
            var shopNotesLookup = dto.ShopNotes?
                .ToDictionary(note => note.ShopId, note => note.Note)
                ?? new Dictionary<int, string?>();

            var createdOrderIds = new List<int>();
            var overallOrderTime = DateTime.UtcNow;

            // === 5. BẮT ĐẦU "SIÊU GIAO DỊCH" (SUPER-TRANSACTION) ===
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // === 6. TÁCH (SPLIT) (DỰA TRÊN CÁC MÓN HÀNG (ITEM) ĐÃ "TICK" (TICKED)) ===
                var cartGroupedByShop = itemsToCheckout.GroupBy(item => item.ShopId);

                // === 7. LẶP (LOOP) QUA TỪNG CỬA HÀNG (SHOP) ĐÃ "TICK" (TICKED) ===
                foreach (var shopGroup in cartGroupedByShop)
                {
                    int shopId = shopGroup.Key;
                    var itemsForThisShop = shopGroup.ToList();
                    decimal totalAmountForThisShop = itemsForThisShop.Sum(i => i.PriceAtTimeOfAdd * i.Quantity);

                    // (Lấy Ghi chú (Note) (Note) "riêng" (specific) (Giải quyết Lỗ hổng 2))
                    shopNotesLookup.TryGetValue(shopId, out var shippingNoteForThisShop);

                    // === 7A. CHECK (KIỂM TRA) "CỔNG 1" & "CỔNG 2" (FCFS (First Come, First Served)) ===

                    var variantIdsForThisShop = itemsForThisShop.Select(i => i.ProductVariantId).ToList();
                    var dbVariants = (await _variantRepo.GetVariantsByIdsAsync(variantIdsForThisShop))
                                        .ToDictionary(v => v.Id);
                    foreach (var item in itemsForThisShop)
                    {
                        if (!dbVariants.TryGetValue(item.ProductVariantId, out var dbVariant))
                        {
                            await transaction.RollbackAsync();
                            return new ApiResponseDTO<CreateOrderResponseDto> { IsSuccess = false, Code = 400, Message = $"Product '{item.ProductName}' is no longer available. Please refresh your cart." };
                        }
                        if (item.PriceAtTimeOfAdd != dbVariant.Price)
                        {
                            await transaction.RollbackAsync();
                            return new ApiResponseDTO<CreateOrderResponseDto> { IsSuccess = false, Code = 400, Message = $"Price for '{item.ProductName}' has changed. Please refresh your cart." };
                        }
                        int rowsAffected = await _context.Database.ExecuteSqlInterpolatedAsync(
                            $@"UPDATE ""PRODUCT_VARIANTS"" 
                               SET ""QUANTITY"" = ""QUANTITY"" - {item.Quantity} 
                               WHERE ""ID"" = {item.ProductVariantId} 
                               AND ""QUANTITY"" >= {item.Quantity}");
                        if (rowsAffected == 0)
                        {
                            await transaction.RollbackAsync();
                            return new ApiResponseDTO<CreateOrderResponseDto> { IsSuccess = false, Code = 400, Message = $"Product '{item.ProductName}' is out of stock. Please refresh your cart." };
                        }
                    }

                    // === 7B. TẠO (CREATE) ORDER (CHA) ===
                    var newOrder = new Order
                    {
                        UserId = customerId,
                        ShopId = shopId,
                        Status = "Pending",
                        TotalAmount = totalAmountForThisShop,
                        PaymentMethod = dto.PaymentMethod,
                        Shipping_Note = shippingNoteForThisShop, // (Gán Ghi chú (Note) (Note) "riêng" (specific))
                        CreatedAt = overallOrderTime,
                        Shipping_FullName = address.FullName,
                        Shipping_Phone = address.Phone,
                        Shipping_AddressLine = address.AddressLine,
                        Shipping_Ward = address.WardName,
                        Shipping_District = address.DistrictName,
                        Shipping_City = address.ProvinceName
                    };
                    await _orderRepo.AddAsync(newOrder);
                    await _orderRepo.SaveChangesAsync();
                    createdOrderIds.Add(newOrder.Id);

                    // === 7C. TẠO (CREATE) N ORDER_ITEM (CON) ===
                    var newOrderItems = new List<OrderItem>();
                    foreach (var item in itemsForThisShop)
                    {
                        newOrderItems.Add(new OrderItem
                        {
                            OrderId = newOrder.Id,
                            Quantity = item.Quantity,
                            ProductVariantId = item.ProductVariantId,
                            PriceAtTimeOfPurchase = item.PriceAtTimeOfAdd,
                            Sku = item.Sku,
                            ProductName = item.ProductName,
                            VariantName = $"{item.ProductName} ({item.Sku}, {dbVariants[item.ProductVariantId].VariantSize}, {dbVariants[item.ProductVariantId].Color})",
                            ImageUrl = item.ImageUrl,
                            CreatedAt = overallOrderTime
                        });
                    }
                    await _orderItemRepo.AddRangeAsync(newOrderItems);
                    await _orderItemRepo.SaveChangesAsync();

                    // === 7D. TẠO (CREATE) 1 PAYMENT (THANH TOÁN) ===
                    var newPayment = new Payment
                    {
                        OrderId = newOrder.Id,
                        Method = dto.PaymentMethod, // "COD"
                        Status = "pending",
                        Amount = newOrder.TotalAmount,
                        CreatedAt = overallOrderTime
                    };
                    await _context.Payments.AddAsync(newPayment);
                    await _context.SaveChangesAsync();

                } // (Kết thúc Lặp (Loop) Cửa hàng (Shop))

                // === 8. COMMIT (CHỐT) "SIÊU GIAO DỊCH" (SUPER-TRANSACTION)! ===
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi Giao dịch (Transaction) khi Tạo Đơn hàng (Order) (Tách (Split)) cho CustomerId {CustomerId}", customerId);
                return new ApiResponseDTO<CreateOrderResponseDto> { IsSuccess = false, Code = 500, Message = "An internal error occurred while placing the order." };
            }

            // === 9. DỌN DẸP (CLEANUP) (BÊN NGOÀI GIAO DỊCH (TRANSACTION)) ===
            try
            {
                await _cartService.RemoveItemsFromCartAsync(customerId, dto.TickedVariantIds);
            }
            catch (Exception redisEx)
            {
                _logger.LogError(redisEx,"Lỗi xoá sản phẩm ra khỏi giỏ hàng của customerID {1}" , customerId);
            }

            // === 10. TRẢ VỀ (RETURN) THÀNH CÔNG ===
            var responseDto = new CreateOrderResponseDto
            {
                CreatedOrderIds = createdOrderIds,
                CreatedAt = overallOrderTime
            };

            return new ApiResponseDTO<CreateOrderResponseDto> { IsSuccess = true, Data = responseDto, Message = "Orders placed successfully." };
        }
        public async Task<PagedListResponseDto<OrderSellerResponseDto>> GetShopOrdersAsync(int userId, OrderFilterDto filter)
        {
            // 1. Lấy ShopId (Logic cũ)
            var shop = await _shop.GetByOwnerIdAsync(userId);
            if (shop == null) throw new Exception("Shop not found"); // Hoặc xử lý lỗi tùy ý

            // 2. Query DB (Repository giữ nguyên logic trả về List và Count)
            // Lưu ý: Repository nhận vào filter.PageNumber và filter.PageSize từ class cha
            var (orders, totalCount) = await _orderRepo.GetOrdersByShopIdAsync(
                shop.Id,
                filter.Status,
                filter.PageNumber,
                filter.PageSize
            );

            // 3. Map sang DTO (Giữ nguyên logic map)
            var dtos = orders.Select(o => new OrderSellerResponseDto
            {
                Id = o.Id,
                Status = o.Status,
                TotalAmount = o.TotalAmount,
                CreatedAt = o.CreatedAt,
                ShippingName = o.Shipping_FullName,
                ShippingAddress=o.Shipping_AddressLine + "," +
                                o.Shipping_Ward + "," +
                                o.Shipping_District + "," +
                                o.Shipping_City,
                ShippingPhone =  o.Shipping_Phone,
                PaymentMethod = o.PaymentMethod,


                // ... map các trường khác ...
                Items = o.OrderItems.Select(i => new OrderItemSellerDto
                {
                    ProductName = i.ProductName,
                    VariantName = i.VariantName,
                    ImageUrl= i.ImageUrl,
                    Price = i.PriceAtTimeOfPurchase,
                    Quantity = i.Quantity

                    // ...
                }).ToList()
            }).ToList();

            // 4. RETURN: Dùng constructor của PagedListResponseDto bạn đã cung cấp
            return new PagedListResponseDto<OrderSellerResponseDto>(
                dtos,
                totalCount,
                filter.PageNumber,
                filter.PageSize
            );
        }
    }
}
