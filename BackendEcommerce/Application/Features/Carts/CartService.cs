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

        // === HÀM "CỘNG DỒN" (GIỮ NGUYÊN) ===
        /// <summary>
        /// (API 2: Thêm/Sửa)
        /// DTO.Quantity là "Số lượng muốn THÊM VÀO" (Delta), không phải Số lượng Tuyệt đối.
        /// </summary>
        public async Task<ApiResponseDTO<int>> AddOrUpdateItemAsync(int customerId, AddCartItemRequestDto dto)
        {
            // 1. (Đọc Redis) Lấy giỏ hàng HIỆN TẠI
            var cart = await GetCartSnapshotAsync(customerId);

            // 2. (Tính toán) Tìm số lượng HIỆN CÓ trong giỏ
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductVariantId == dto.ProductVariantId);
            int currentQuantityInCart = (existingItem != null) ? existingItem.Quantity : 0;

            // 3. (Tính toán) Tính TỔNG MỚI mà User muốn
            int newTotalQuantity = currentQuantityInCart + dto.Quantity; // <-- LOGIC CỘNG DỒN

            // 4. (Check DB) Check Tồn kho
            var variant = await _variantRepo.GetByIdAsync(dto.ProductVariantId); // (Đã sửa, có Include Product)
            if (variant == null)
            {
                return new ApiResponseDTO<int> { IsSuccess = false, Code = 404, Message = "Sản phẩm không tồn tại." };
            }

            // (Check Tồn kho với TỔNG MỚI, không phải với dto.Quantity)
            if (variant.Quantity < newTotalQuantity)
            {
                return new ApiResponseDTO<int> { IsSuccess = false, Code = 400, Message = $"Số lượng tồn kho không đủ (Chỉ còn {variant.Quantity})." };
            }

            // 5. (Logic "Cộng dồn" / "Thêm mới")
            if (existingItem != null)
            {
                // Case A: Đã có -> Cập nhật TỔNG MỚI (Đã sửa từ '=')
                existingItem.Quantity = newTotalQuantity;
            }
            else
            {
                // Case B: Món mới -> "Snapshot" (Sao chép)
                // (Lấy ảnh "con" (variant) trước)
                var media = await _mediaRepo.GetMediaForVariantAsync(variant.Id);

                var newItem = new CartItemSnapshotDto
                {
                    ProductVariantId = variant.Id,
                    Quantity = newTotalQuantity, // (Dùng TỔNG MỚI)
                    PriceAtTimeOfAdd = variant.Price,
                    Sku = variant.SKU,
                    ProductName = variant.Product.Name, // (Sẽ không null vì GetByIdAsync đã Include)
                    ImageUrl = media?.ImageUrl ?? "https://placehold.co/100x100?text=No+Image"
                };
                cart.Items.Add(newItem);
            }

            // 6. (Ghi Redis)
            var updatedJson = JsonSerializer.Serialize(cart);
            await _redisDb.StringSetAsync(GetCartKey(customerId), updatedJson);

            // 7. (Trả về "Count" - theo yêu cầu "Tinh gọn")
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
        /// <summary>
        /// (API 6: Xóa tất cả sản phẩm trong giỏ hàng)
        /// </summary>
        public async Task<ApiResponseDTO<int>> ClearCartAsync(int customerId)
        {
            var key = GetCartKey(customerId);

            // 1. (Đọc Redis) Lấy key về để check xem nó có tồn tại không
            var cartExists = await _redisDb.KeyExistsAsync(key);

            if (cartExists)
            {
                // 2. (Ghi Redis) Xóa Key
                await _redisDb.KeyDeleteAsync(key);
            }

            // 3. (Trả về "Count" - luôn là 0)
            return new ApiResponseDTO<int> { IsSuccess = true, Data = 0 };
        }
        // === HÀM "GHI ĐÈ" (MỚI) ===
        /// <summary>
        /// (API MỚI) "Ghi đè" (Set) số lượng tuyệt đối
        /// (Dùng cho [PUT] /api/cart/items)
        /// </summary>
        public async Task<ApiResponseDTO<int>> SetItemQuantityAsync(int customerId, UpdateCartItemRequestDto dto)
        {
            // 1. (Check DB) Check Tồn kho
            var variant = await _variantRepo.GetByIdAsync(dto.ProductVariantId);
            if (variant == null)
            {
                return new ApiResponseDTO<int> { IsSuccess = false, Code = 404, Message = "Sản phẩm không tồn tại." };
            }

            // (Check Tồn kho với số lượng "Ghi đè" (NewQuantity))
            if (variant.Quantity < dto.NewQuantity)
            {
                return new ApiResponseDTO<int> { IsSuccess = false, Code = 400, Message = $"Số lượng tồn kho không đủ (Chỉ còn {variant.Quantity})." };
            }

            // 2. (Đọc Redis)
            var cart = await GetCartSnapshotAsync(customerId);

            // 3. (Logic "Ghi đè")
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductVariantId == dto.ProductVariantId);

            if (existingItem != null)
            {
                // Case A: Đã có -> GHI ĐÈ
                existingItem.Quantity = dto.NewQuantity;
            }
            else
            {
                // Case B: Món này chưa có trong giỏ? (Lạ, nhưng cứ xử lý)
                // (Đây là logic "Snapshot" y hệt AddOrUpdateItemAsync)
                var media = await _mediaRepo.GetMediaForVariantAsync(variant.Id);
                var newItem = new CartItemSnapshotDto
                {
                    ProductVariantId = variant.Id,
                    Quantity = dto.NewQuantity, // Dùng số lượng Ghi đè
                    PriceAtTimeOfAdd = variant.Price,
                    Sku = variant.SKU,
                    ProductName = variant.Product.Name,
                    ImageUrl = media?.ImageUrl ?? "https://placehold.co/100x100?text=No+Image"
                };
                cart.Items.Add(newItem);
            }

            // 4. (Ghi Redis)
            var updatedJson = JsonSerializer.Serialize(cart);
            await _redisDb.StringSetAsync(GetCartKey(customerId), updatedJson);

            // 5. (Trả về "Count" - theo yêu cầu "Tinh gọn")
            return new ApiResponseDTO<int> { IsSuccess = true, Data = cart.TotalItemsCount };
        }
    }
}
