using BackendEcommerce.Application.Features.Carts.Contracts;
using BackendEcommerce.Application.Features.Carts.DTOs;
using BackendEcommerce.Application.Features.Medias.Contracts;
using BackendEcommerce.Application.Features.Products.Contracts;
using BackendEcommerce.Application.Shared.DTOs;
using StackExchange.Redis;
using System.Text.Json;

namespace BackendEcommerce.Application.Features.Carts
{
    public class CartService : ICartService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IProductVariantRepository _variantRepo;
        private readonly ILogger<CartService> _logger;
        private readonly IMediaRepository _mediaRepo; 
        private readonly IDatabase _redisDb; // (Biến CSDL Redis)

        // Key (Redis)
        private string GetCartKey(int customerId) => $"cart:{customerId}";

        public CartService(
            IConnectionMultiplexer redis,
            IProductVariantRepository variantRepo,
            IMediaRepository mediaRepo,
            ILogger<CartService> logger)
        {
            _redis = redis;
            _variantRepo = variantRepo;
            _logger = logger;
            _mediaRepo = mediaRepo;
            _redisDb = _redis.GetDatabase(); // (Khởi tạo CSDL Redis)
        }

        // === API 1 (Nội bộ): Lấy Snapshot (Nhanh) ===
        public async Task<CartSnapshotDto> GetCartSnapshotAsync(int customerId)
        {
            var key = GetCartKey(customerId);
            var jsonCart = await _redisDb.StringGetAsync(key);

            if (jsonCart.IsNullOrEmpty)
            {
                // KỊCH BẢN 1 (Rỗng): Trả về Giỏ hàng rỗng (để trả về Count = 0)
                return new CartSnapshotDto();
            }

            try
            {
                // Trả về Giỏ hàng "Snapshot" (Cũ/Stale)
                return JsonSerializer.Deserialize<CartSnapshotDto>(jsonCart!)
                       ?? new CartSnapshotDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi Deserialize JSON Giỏ hàng (Redis) cho CustomerID {CustomerId}", customerId);
                return new CartSnapshotDto(); // Trả về rỗng nếu JSON bị hỏng
            }
        }

        // === API 2 & 3 (Lệnh Thêm/Sửa): AddOrUpdateItemAsync ===
        public async Task<ApiResponseDTO<int>> AddOrUpdateItemAsync(int customerId, AddCartItemRequestDto dto)
        {
            // === CỔNG 1: CHECK TỒN KHO (Oracle DB) ===
            var variant = await _variantRepo.GetByIdAsync(dto.ProductVariantId);

            // Check 404
            if (variant == null)
            {
                return new ApiResponseDTO<int> { IsSuccess = false, Code = 404, Message = "Sản phẩm không tồn tại." };
            }
            // Check 400 (Tồn kho)
            if (variant.Quantity < dto.Quantity)
            {
                return new ApiResponseDTO<int> { IsSuccess = false, Code = 400, Message = $"Số lượng tồn kho không đủ (Chỉ còn {variant.Quantity} sản phẩm)." };
            }

            // === LOGIC REDIS ===
            var key = GetCartKey(customerId);
            var cart = await GetCartSnapshotAsync(customerId); // (Dùng hàm nội bộ ở trên)

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductVariantId == dto.ProductVariantId);

            if (existingItem != null)
            {
                // Case B (Cộng dồn/Update): Đã có -> Cập nhật Số lượng
                // (Vẫn phải check Tồn kho lại 1 lần nữa)
                if (dto.Quantity > variant.Quantity)
                {
                    return new ApiResponseDTO<int> { IsSuccess = false, Code = 400, Message = $"Số lượng tồn kho không đủ (Chỉ còn {variant.Quantity} sản phẩm)." };
                }
                existingItem.Quantity = dto.Quantity;
            }
            else
            {
                // Case A (Thêm mới): Tạo "Snapshot"
                var media = await _mediaRepo.GetMediaForVariantAsync(dto.ProductVariantId); // (Giả định tiêm IMediaRepo)
                var snapshotItem = new CartItemSnapshotDto
                {
                    ProductVariantId = variant.Id,
                    Quantity = dto.Quantity,
                    // --- Dữ liệu "Snapshot" ---
                    PriceAtTimeOfAdd = variant.Price,
                    ProductName = variant.Product.Name, // (Giả định GetByIdAsync đã Include Product)
                    Sku = variant.SKU,
                    ImageUrl = media?.ImageUrl ?? "https://placehold.co/100x100?text=No+Image"
                };
                cart.Items.Add(snapshotItem);
            }

            // Lưu (Ghi đè) Giỏ hàng mới vào Redis
            var newJsonCart = JsonSerializer.Serialize(cart);
            await _redisDb.StringSetAsync(key, newJsonCart);

            // Trả về (return) CHỈ con số Count (theo yêu cầu)
            return new ApiResponseDTO<int> { IsSuccess = true, Data = cart.TotalItemsCount };
        }

        // === API 4 (Lệnh Xóa): DeleteItemAsync ===
        public async Task<ApiResponseDTO<int>> DeleteItemAsync(int customerId, int variantId)
        {
            var key = GetCartKey(customerId);
            var cart = await GetCartSnapshotAsync(customerId);

            var itemToRemove = cart.Items.FirstOrDefault(i => i.ProductVariantId == variantId);

            if (itemToRemove != null)
            {
                cart.Items.Remove(itemToRemove);

                // Lưu (Ghi đè) Giỏ hàng mới vào Redis
                var newJsonCart = JsonSerializer.Serialize(cart);
                await _redisDb.StringSetAsync(key, newJsonCart);
            }

            // Trả về (return) CHỈ con số Count (theo yêu cầu)
            return new ApiResponseDTO<int> { IsSuccess = true, Data = cart.TotalItemsCount };
        }

        // === API 5 (Query "Chậm" - Refresh): GetAndRefreshCartAsync ===
        public async Task<ApiResponseDTO<CartSnapshotDto>> GetAndRefreshCartAsync(int customerId)
        {
            // 1. Lấy Giỏ hàng "Cũ" (Stale) từ Redis
            var key = GetCartKey(customerId);
            var cart = await GetCartSnapshotAsync(customerId);

            if (!cart.Items.Any())
            {
                // Giỏ hàng rỗng, không cần "Refresh"
                return new ApiResponseDTO<CartSnapshotDto> { IsSuccess = true, Data = cart };
            }

            // 2. Lấy ID và gọi Oracle DB 1 lần
            var variantIds = cart.Items.Select(i => i.ProductVariantId).ToList();
            // (Giả định IProductVariantRepository có hàm GetByIdsAsync)
            var dbVariants = await _variantRepo.GetVariantsByIdsAsync(variantIds);
            var dbVariantsMap = dbVariants.ToDictionary(v => v.Id);

            bool cartWasModified = false; // (Cờ (Flag) để check)
            var itemsToRemove = new List<CartItemSnapshotDto>(); // (List tạm để xóa)

            // 3. Lặp (Loop) và "Refresh" (Đồng bộ)
            foreach (var item in cart.Items)
            {
                if (dbVariantsMap.TryGetValue(item.ProductVariantId, out var dbVariant))
                {
                    // TÌM THẤY (So sánh)
                    // a. Check Giá
                    if (item.PriceAtTimeOfAdd != dbVariant.Price)
                    {
                        item.PriceAtTimeOfAdd = dbVariant.Price; // Cập nhật giá mới
                        cartWasModified = true;
                    }
                    // b. Check Tồn kho
                    if (item.Quantity > dbVariant.Quantity)
                    {
                        item.Quantity = dbVariant.Quantity; // Giảm số lượng xuống mức tồn kho
                        cartWasModified = true;
                    }
                }
                else
                {
                    // KHÔNG TÌM THẤY (Sản phẩm đã bị Xóa Cứng)
                    itemsToRemove.Add(item); // Đánh dấu để xóa khỏi giỏ
                    cartWasModified = true;
                }
            }

            // (Xóa các item "mồ côi")
            foreach (var itemToRemove in itemsToRemove)
            {
                cart.Items.Remove(itemToRemove);
            }

            // 4. Lưu (Ghi đè) Giỏ hàng (đã "refresh") trở lại Redis
            if (cartWasModified)
            {
                var newJsonCart = JsonSerializer.Serialize(cart);
                await _redisDb.StringSetAsync(key, newJsonCart);
            }

            // 5. Trả về Giỏ hàng (đã "refresh")
            return new ApiResponseDTO<CartSnapshotDto> { IsSuccess = true, Data = cart };
        }
    }
}
