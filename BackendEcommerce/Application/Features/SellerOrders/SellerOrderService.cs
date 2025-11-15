using BackendEcommerce.Application.Features.Orders.Contracts;
using BackendEcommerce.Application.Features.Orders.DTOs;
using BackendEcommerce.Application.Features.Products.Contracts;
using BackendEcommerce.Application.Features.SellerOrders.Contracts;
using BackendEcommerce.Application.Shared.DTOs;
using BackendEcommerce.Infrastructure.Persistence.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Authentication;

namespace BackendEcommerce.Application.Features.SellerOrders
{
    public class SellerOrderService : ISellerOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IShopRepository _shopRepo;

        public SellerOrderService(IOrderRepository orderRepo, IShopRepository shopRepo)
        {
            _orderRepo = orderRepo;
            _shopRepo = shopRepo;
        }

        // ... (Hàm GetShopOrdersAsync giữ nguyên như cũ...)
        public async Task<PagedListResponseDto<OrderSellerResponseDto>> GetShopOrdersAsync(int userId, OrderFilterDto filter)
        {
            // 1. Lấy ShopId từ UserId
            var shop = await _shopRepo.GetByOwnerIdAsync(userId);
            if (shop == null)
            {
                return new PagedListResponseDto<OrderSellerResponseDto>(new List<OrderSellerResponseDto>(), 0, filter.PageNumber, filter.PageSize);
            }

            // 2. Gọi Repository
            var (orders, totalCount) = await _orderRepo.GetOrdersByShopIdAsync(
                shop.Id,
                filter.Status,
                filter.PageNumber,
                filter.PageSize
            );

            // 3. Map
            var dtos = orders.Select(o => MapToSellerDto(o)).ToList();

            // 4. Trả về
            return new PagedListResponseDto<OrderSellerResponseDto>(
                dtos,
                totalCount,
                filter.PageNumber,
                filter.PageSize
            );
        }

        /// <summary>
        /// 2. Cập nhật trạng thái đơn hàng (ĐÃ SỬA LẠI LOGIC UoW)
        /// </summary>
        public async Task<OrderSellerResponseDto> UpdateOrderStatusAsync(int userId, int orderId, string newStatus)
        {
            // 1. Lấy ShopId
            var shop = await _shopRepo.GetByOwnerIdAsync(userId);
            if (shop == null)
            {
                throw new AuthenticationException("Bạn không có quyền thực hiện hành động này (Không tìm thấy Shop).");
            }

            // 2. Lấy đơn hàng (Sử dụng hàm mới, CÓ tracking)
            var order = await _orderRepo.GetOrderByIdWithItemsAsync(orderId);
            if (order == null)
            {
                throw new KeyNotFoundException("Không tìm thấy đơn hàng.");
            }

            // 3. KIỂM TRA BẢO MẬT
            if (order.ShopId != shop.Id)
            {
                throw new AuthenticationException("Bạn không có quyền cập nhật đơn hàng này (Không đúng chủ Shop).");
            }

            // 4. LOGIC STATE MACHINE (Giữ nguyên)
            var currentStatus = order.Status.ToLower();
            var targetStatus = newStatus.ToLower();

            bool isValidTransition = false;
            switch (currentStatus)
            {
                case "pending":
                    if (targetStatus == "processing" || targetStatus == "cancelled")
                        isValidTransition = true;
                    break;
                case "processing":
                    if (targetStatus == "shipped" || targetStatus == "cancelled")
                        isValidTransition = true;
                    break;
                case "shipped":
                case "completed":
                case "cancelled":
                    isValidTransition = false;
                    break;
            }

            if (!isValidTransition)
            {
                throw new ValidationException($"Không thể chuyển đổi trạng thái từ '{currentStatus}' sang '{targetStatus}'.");
            }

            // 6. Cập nhật và Lưu (ĐÃ SỬA)
            order.Status = targetStatus;
            order.UpdatedAt = DateTime.UtcNow;

            // 6a. (SỬA) Gọi hàm Update đồng bộ (để đánh dấu Entity)
            _orderRepo.Update(order);

            // 6b. (SỬA) Gọi SaveChangesAsync để áp dụng (đây là Unit of Work)
            await _orderRepo.SaveChangesAsync();

            // 7. Map và trả về
            return MapToSellerDto(order);
        }

        // --- PRIVATE HELPER (Giữ nguyên) ---
        private OrderSellerResponseDto MapToSellerDto(Order order)
        {
            return new OrderSellerResponseDto
            {
                Id = order.Id,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                PaymentMethod = order.PaymentMethod,
                CreatedAt = order.CreatedAt,
                ShippingName = order.Shipping_FullName,
                ShippingPhone = order.Shipping_Phone,
                ShippingAddress = $"{order.Shipping_AddressLine}, {order.Shipping_Ward}, {order.Shipping_District}, {order.Shipping_City}",
                Items = order.OrderItems.Select(i => new OrderItemSellerDto
                {
                    ProductName = i.ProductName,
                    VariantName = i.VariantName,
                    Sku = i.Sku,
                    Quantity = i.Quantity,
                    Price = i.PriceAtTimeOfPurchase,
                    ImageUrl = i.ImageUrl
                }).ToList()
            };
        }
    }
}
