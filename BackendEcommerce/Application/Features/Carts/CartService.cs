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
        private const string CartKeyPrefix = "cart:";

        public CartService(
            IConnectionMultiplexer redis, // (Tiêm Client Redis)
            IProductVariantRepository variantRepo,
            IMediaRepository mediaRepo,
            ILogger<CartService> logger)
        {
            _redisDb = redis.GetDatabase();
            _variantRepo = variantRepo;
            _mediaRepo = mediaRepo;
            _logger = logger;
        }

        private string GetCartKey(int customerId) => $"{CartKeyPrefix}{customerId}";

        // (API (Giao diện Lập trình Ứng dụng) 1: Dùng cho /me - Vẫn trả về (return) DTO (Đối tượng Truyền dữ liệu) "phẳng" (flat))
        public async Task<CartSnapshotDto> GetCartSnapshotAsync(int customerId)
        {
            var cartJson = await _redisDb.StringGetAsync(GetCartKey(customerId));
            if (cartJson.IsNullOrEmpty)
            {
                return new CartSnapshotDto(); // (Giỏ hàng (Cart) (Cart) rỗng (empty))
            }

            try
            {
                return JsonSerializer.Deserialize<CartSnapshotDto>(cartJson!) ?? new CartSnapshotDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi Deserialize (Giải mã) Giỏ hàng (Cart) (Cart) Redis cho CustomerId {CustomerId}", customerId);
                return new CartSnapshotDto(); // (An toàn)
            }
        }

        /// <summary>
        /// (API (Giao diện Lập trình Ứng dụng) 4: "Refresh" (Làm mới) Chậm)
        /// (ĐÃ CẬP NHẬT (UPDATED): Để trả về (return) DTO (Đối tượng Truyền dữ liệu) "Đã Gom nhóm" (Grouped) MỚI)
        /// </summary>
        public async Task<ApiResponseDTO<GroupedCartResponseDto>> GetAndRefreshCartAsync(int customerId)
        {
            // === 1. LẤY DTO (Đối tượng Truyền dữ liệu) (DTO (Đối tượng Truyền dữ liệu)) "PHẲNG" (FLAT) (CŨ) (STALE) TỪ REDIS ===
            var cart = await GetCartSnapshotAsync(customerId);

            if (!cart.Items.Any())
            {
                // (Nếu giỏ (cart) rỗng (empty), trả về (return) 1 DTO (Đối tượng Truyền dữ liệu) (DTO (Đối tượng Truyền dữ liệu)) "Gom nhóm" (Grouped) rỗng (empty))
                return new ApiResponseDTO<GroupedCartResponseDto> { IsSuccess = true, Data = new GroupedCartResponseDto() };
            }

            // === 2. LẤY ID (Mã) ===
            var variantIds = cart.Items.Select(i => i.ProductVariantId).ToList();

            // === 3. GỌI ORACLE DB (1 LẦN) (ĐỂ "REFRESH" (LÀM MỚI)) ===
            // (Hàm này (GetVariantsByIdsAsync) giờ đã Include(v => v.Product.Shop))
            var dbVariants = await _variantRepo.GetVariantsByIdsAsync(variantIds);
            var dbVariantsMap = dbVariants.ToDictionary(v => v.Id);

            bool cartWasModified = false;
            string cartModificationMessage = "OK";

            // === 4. ĐỒNG BỘ (SYNC) REDIS VS DB ===
            // (Lặp (Loop) ngược (backward) để Xóa (Remove) an toàn)
            for (int i = cart.Items.Count - 1; i >= 0; i--)
            {
                var item = cart.Items[i];

                // (Check (Kiểm tra) 1: Sản phẩm (Product) bị Xóa Cứng (Hard Deleted)?)
                if (!dbVariantsMap.TryGetValue(item.ProductVariantId, out var dbVariant))
                {
                    cart.Items.RemoveAt(i); // (Xóa (Remove) khỏi Giỏ hàng (Cart) (Cart))
                    cartModificationMessage = "Some items were removed.";
                    cartWasModified = true;
                    continue;
                }

                // (Check (Kiểm tra) 2: Tồn kho (Stock) (Stock) thay đổi?)
                if (item.Quantity > dbVariant.Quantity)
                {
                    item.Quantity = dbVariant.Quantity; // (Giảm (Reduce) số lượng (quantity))
                    cartModificationMessage = "Cart was modified";
                    cartWasModified = true;
                }

                // (Check (Kiểm tra) 3: Giá (Price) (Price) thay đổi?)
                if (item.PriceAtTimeOfAdd != dbVariant.Price)
                {
                    item.PriceAtTimeOfAdd = dbVariant.Price; // (Cập nhật (Update) "Snapshot" (Sao chép) Giá (Price) (Price))
                    cartModificationMessage = "Cart was modified";
                    cartWasModified = true;
                }

                // (Check (Kiểm tra) 4: Tên (Name)/Shop (Cửa hàng) thay đổi?)
                item.ProductName = dbVariant.Product.Name;
                item.ShopId = dbVariant.Product.ShopId;
                item.ShopName = dbVariant.Product.Shop.Name;
            }

            // === 5. CẬP NHẬT (UPDATE) REDIS (NẾU CÓ THAY ĐỔI) ===
            if (cartWasModified)
            {
                var updatedJson = JsonSerializer.Serialize(cart);
                await _redisDb.StringSetAsync(GetCartKey(customerId), updatedJson);
            }

            // === 6. "GOM NHÓM" (GROUPING) (THEO YÊU CẦU MỚI) ===
            // (Biến đổi (Transform) DTO (Đối tượng Truyền dữ liệu) (DTO (Đối tượng Truyền dữ liệu)) "phẳng" (flat) (cart) thành DTO (Đối tượng Truyền dữ liệu) (DTO (Đối tượng Truyền dữ liệu)) "gom nhóm" (grouped))
            var groupedResponse = new GroupedCartResponseDto
            {
                Shops = cart.Items
                    .GroupBy(item => new { item.ShopId, item.ShopName }) // (Gom (Group) theo Cửa hàng (Shop))
                    .Select(shopGroup => new ShopCartGroupDto
                    {
                        ShopId = shopGroup.Key.ShopId,
                        ShopName = shopGroup.Key.ShopName,
                        Items = shopGroup.ToList() // (List (Danh sách) món hàng (item) của Cửa hàng (Shop) này)
                    })
                    .ToList()
            };

            return new ApiResponseDTO<GroupedCartResponseDto> { IsSuccess = true, Data = groupedResponse, Message = cartModificationMessage };
        }

        // === API (Giao diện Lập trình Ứng dụng) "CỘNG DỒN" (ACCUMULATE) (VẪN GIỮ NGUYÊN) ===
        public async Task<ApiResponseDTO<int>> AddOrUpdateItemAsync(int customerId, AddCartItemRequestDto dto)
        {
            var cart = await GetCartSnapshotAsync(customerId);
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductVariantId == dto.ProductVariantId);
            int currentQuantityInCart = (existingItem != null) ? existingItem.Quantity : 0;
            int newTotalQuantity = currentQuantityInCart + dto.Quantity;

            var variant = await _variantRepo.GetByIdAsync(dto.ProductVariantId);
            if (variant == null || variant.Product == null || variant.Product.Shop == null)
            {
                return new ApiResponseDTO<int> { IsSuccess = false, Code = 404, Message = "Sản phẩm không tồn tại." };
            }

            if (variant.Quantity < newTotalQuantity)
            {
                return new ApiResponseDTO<int> { IsSuccess = false, Code = 400, Message = $"Số lượng tồn kho không đủ (Chỉ còn {variant.Quantity})." };
            }

            if (existingItem != null)
            {
                existingItem.Quantity = newTotalQuantity;
                existingItem.ShopName = variant.Product.Shop.Name;
            }
            else
            {
                var media = await _mediaRepo.GetMediaForVariantAsync(variant.Id);
                var newItem = new CartItemSnapshotDto
                {
                    ProductVariantId = variant.Id,
                    Quantity = newTotalQuantity,
                    PriceAtTimeOfAdd = variant.Price,
                    Sku = variant.SKU,
                    ProductName = variant.Product.Name,
                    ImageUrl = media?.ImageUrl ?? "https://placehold.co/100x100?text=No+Image",
                    ShopId = variant.Product.ShopId,
                    ShopName = variant.Product.Shop.Name
                };
                cart.Items.Add(newItem);
            }

            var updatedJson = JsonSerializer.Serialize(cart);
            await _redisDb.StringSetAsync(GetCartKey(customerId), updatedJson);

            return new ApiResponseDTO<int> { IsSuccess = true, Data = cart.TotalItemsCount };
        }

        // === API (Giao diện Lập trình Ứng dụng) "GHI ĐÈ" (SET) (VẪN GIỮ NGUYÊN) ===
        public async Task<ApiResponseDTO<int>> SetItemQuantityAsync(int customerId, UpdateCartItemRequestDto dto)
        {
            var cart = await GetCartSnapshotAsync(customerId);
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductVariantId == dto.ProductVariantId);

            if (existingItem == null)
            {
                return new ApiResponseDTO<int> { IsSuccess = false, Code = 404, Message = "Sản phẩm không có trong giỏ (cart) của bạn." };
            }

            var variant = await _variantRepo.GetByIdAsync(dto.ProductVariantId);
            if (variant == null)
            {
                return new ApiResponseDTO<int> { IsSuccess = false, Code = 404, Message = "Sản phẩm không tồn tại." };
            }

            if (variant.Quantity < dto.NewQuantity)
            {
                return new ApiResponseDTO<int> { IsSuccess = false, Code = 400, Message = $"Số lượng tồn kho không đủ (Chỉ còn {variant.Quantity})." };
            }

            // Logic (Lô-gic) "Ghi đè" (Set)
            existingItem.Quantity = dto.NewQuantity;

            var updatedJson = JsonSerializer.Serialize(cart);
            await _redisDb.StringSetAsync(GetCartKey(customerId), updatedJson);

            return new ApiResponseDTO<int> { IsSuccess = true, Data = cart.TotalItemsCount };
        }


        // === API (Giao diện Lập trình Ứng dụng) "XÓA 1" (DELETE 1) (VẪN GIỮ NGUYÊN) ===
        public async Task<ApiResponseDTO<int>> DeleteItemAsync(int customerId, int variantId)
        {
            var cart = await GetCartSnapshotAsync(customerId);

            var itemToRemove = cart.Items.FirstOrDefault(i => i.ProductVariantId == variantId);
            if (itemToRemove != null)
            {
                cart.Items.Remove(itemToRemove);

                var updatedJson = JsonSerializer.Serialize(cart);
                await _redisDb.StringSetAsync(GetCartKey(customerId), updatedJson);
            }

            return new ApiResponseDTO<int> { IsSuccess = true, Data = cart.TotalItemsCount };
        }

        // === API (Giao diện Lập trình Ứng dụng) "XÓA TẤT CẢ" (CLEAR ALL) (VẪN GIỮ NGUYÊN) ===
        public async Task<ApiResponseDTO<int>> ClearCartAsync(int customerId)
        {
            var key = GetCartKey(customerId);

            var cartExists = await _redisDb.KeyExistsAsync(key);

            if (cartExists)
            {
                await _redisDb.KeyDeleteAsync(key);
            }

            return new ApiResponseDTO<int> { IsSuccess = true, Data = 0 };
        }

        /// <summary>
        /// (HÀM MỚI (NEW) (API (Giao diện Lập trình Ứng dụng) 6): Dọn dẹp (Cleanup) (Cleanup) (Dọn dẹp) (Cleanup) (Dọn dẹp) (Cleanup) (Dọn dẹp) (Cleanup) (Dùng bởi OrderService (Dịch vụ Đơn hàng))
        /// </summary>
        public async Task RemoveItemsFromCartAsync(int customerId, List<int> variantIdsToRemove)
        {
            var cart = await GetCartSnapshotAsync(customerId);

            // (Lọc (Filter) (Filter) (Lọc (Filter)) (Filter) (Lọc (Filter)) (Filter) (Lọc (Filter)) (Filter) ra các món hàng (item) (item) (món hàng (item)) (item) (món hàng (item)) (item) (món hàng (item)) (item) KHÔNG (NOT) (NOT) (KHÔNG (NOT)) (NOT) (KHÔNG (NOT)) (NOT) (KHÔNG (NOT)) (NOT) bị xóa (remove))
            cart.Items = cart.Items
                .Where(item => !variantIdsToRemove.Contains(item.ProductVariantId))
                .ToList();

            if (!cart.Items.Any())
            {
                // (Nếu Giỏ hàng (Cart) (Cart) rỗng (empty), Xóa (Remove) (Remove) (Xóa (Remove)) (Remove) (Xóa (Remove)) (Remove) (Xóa (Remove)) (Remove) Key (Khóa) (Khóa) (Key (Khóa)) (Khóa) (Key (Khóa)) (Khóa) (Key (Khóa)) (Khóa) cho "sạch" (clean))
                await _redisDb.KeyDeleteAsync(GetCartKey(customerId));
            }
            else
            {
                // (Lưu (Save) (Save) (Lưu (Save)) (Save) (Lưu (Save)) (Save) (Lưu (Save)) (Save) Giỏ hàng (Cart) (Cart) (đã dọn dẹp (cleanup) (cleanup) (dọn dẹp (cleanup)) (cleanup) (dọn dẹp (cleanup)) (cleanup) (dọn dẹp (cleanup)) (cleanup)))
                var updatedJson = JsonSerializer.Serialize(cart);
                await _redisDb.StringSetAsync(GetCartKey(customerId), updatedJson);
            }
        }
    }
}

