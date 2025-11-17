using BackendEcommerce.Application.Features.Products.DTOs;
using BackendEcommerce.Application.Features.Medias.DTOs;
using BackendEcommerce.Application.Shared.DTOs;
using BackendEcommerce.Application.Shared.Validations;
using BackendEcommerce.Infrastructure.Persistence.Data;
using BackendEcommerce.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore.Storage;
using System.Diagnostics;
using BackendEcommerce.Application.Features.Categories.Contracts;
using BackendEcommerce.Application.Features.Medias.Contracts;
using BackendEcommerce.Application.Features.Products.Contracts;
using BackendEcommerce.Application.Shared.Contracts;
namespace BackendEcommerce.Application.Features.Products
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepo;
        private readonly IShopRepository _shopRepo;
        private readonly IMediaRepository _mediaRepo;
        private readonly IMediaUploadService _mediaUploadService;
        private readonly IProductVariantRepository _variantRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly EcomDbContext _context; // <-- Thêm (chỉ để dùng Transaction)
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IProductRepository productRepo,
            IShopRepository shopRepo,
            IMediaRepository mediaRepo,
            IMediaUploadService mediaUploadService,
            IProductVariantRepository variantRepo,
            ICategoryRepository categoryRepo,
            ILogger<ProductService> logger,
            EcomDbContext context)
        {
            _productRepo = productRepo;
            _variantRepo = variantRepo;
            _shopRepo = shopRepo;
            _mediaRepo = mediaRepo;
            _mediaUploadService = mediaUploadService;
            _categoryRepo = categoryRepo;
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponseDTO<CreateProductResponseDto>> CreateProductAsync(CreateProductRequestDto dto, int sellerId)
        {
            // === 1. PHÂN QUYỀN (Authorization Check) ===
            var shop = await _shopRepo.GetByOwnerIdAsync(sellerId);
            if (shop == null)
            {
                return new ApiResponseDTO<CreateProductResponseDto>
                {
                    IsSuccess = false,
                    Code = 403,
                    Message = "Người dùng không phải là seller"
                };
            }
            // 1. Check trùng SKU (SKU bị trùng ngay trong DTO gửi lên)
            var duplicateSkus = dto.Variants
                .GroupBy(v => v.SKU)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            if (duplicateSkus.Any())
            {
                return new ApiResponseDTO<CreateProductResponseDto>
                {
                    IsSuccess = false,
                    Code = 400,
                    Message = $"Các SKU bị trùng lặp trong request: {string.Join(", ", duplicateSkus)}"
                };
            }

            // 2. Check CategoryId có tồn tại không
            // (Giả định ICategoryRepository có hàm GetByIdAsync)
            var category = await _categoryRepo.GetByIdAsync(dto.CategoryId);
            if (category == null)
            {
                return new ApiResponseDTO<CreateProductResponseDto>
                {
                    IsSuccess = false,
                    Code = 400,
                    Message = $"CategoryId '{dto.CategoryId}' không tồn tại."
                };
            }
            // Dùng để rollback Cloudinary
            var uploadedMediaResults = new List<MediaUploadResult>();
            // Dùng để lưu vào DB
            var mediaEntitiesToSave = new List<Media>();

            // BẮT BUỘC DÙNG TRANSACTION (Vì ta ghi vào 2-3 bảng khác nhau)
            await using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Tạo Product (Cha)
                var newProduct = new Product
                {
                    ShopId = shop.Id,
                    CategoryId = dto.CategoryId, // Đã check
                    Name = dto.Name,
                    Description = dto.Description,
                    Brand = dto.Brand,
                    Status = "active",
                    CreatedAt = DateTime.UtcNow,
                    VariantCount = dto.Variants.Count,
                    MinPrice = dto.Variants.Min(v => v.Price)
                };
                //  Xử lý Ảnh của Product (Cha)
                    int productImageIndex = 0;
                    foreach (var file in dto.ProductImages)
                    {
                        var uploadResult = await _mediaUploadService.UploadImageAsync(file);
                        uploadedMediaResults.Add(uploadResult);

                        mediaEntitiesToSave.Add(new Media
                        {
                            EntityType = "product",
                            ImageUrl = uploadResult.Url,
                            PublicId = uploadResult.PublicId,
                            IsPrimary = productImageIndex == 0, // Ảnh đầu tiên là ảnh chính
                            CreatedAt = DateTime.UtcNow
                        });
                        productImageIndex++;
                    }
                // ===  Upload và Chuẩn bị Media cho Variant (Level) ===
                foreach (var variantDto in dto.Variants)
                {
                    // === BỔ SUNG VALIDATION MỚI (THEO YÊU CẦU) ===
                    var validationError = VariantValidator.ValidateAttributes(variantDto.VariantSize, variantDto.Color);
                    if (validationError != null)
                    {
                        await transaction.RollbackAsync(); // Hủy transaction
                        return new ApiResponseDTO<CreateProductResponseDto>
                        {
                            IsSuccess = false,
                            Code = 400,
                            Message = $"Variant (SKU: {variantDto.SKU}): {validationError}" // Thêm SKU để FE biết lỗi ở đâu
                        };
                    }
                    // a. Upload ảnh
                    var variantUploadResult = await _mediaUploadService.UploadImageAsync(variantDto.Image);
                    uploadedMediaResults.Add(variantUploadResult); // Thêm vào list rollback
                    // b. Tạo Variant Entity
                    var newVariant = new ProductVariant
                    {
                        SKU = variantDto.SKU,
                        VariantSize = variantDto.VariantSize,
                        Color = variantDto.Color,
                        Price = variantDto.Price,
                        Material= variantDto.Material,
                        Quantity = variantDto.Quantity,
                        CreatedAt = DateTime.UtcNow
                    };
                    // c. Thêm Variant vào Product
                    newProduct.Variants.Add(newVariant);
                    // d. Chuẩn bị Media cho Variant (chưa gán Id)
                    mediaEntitiesToSave.Add(new Media
                    {
                        EntityType = "variant",
                        // EntityId sẽ được gán sau khi SaveChanges() Variant
                        ImageUrl = variantUploadResult.Url,
                        PublicId = variantUploadResult.PublicId,
                        IsPrimary = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                // ===  LƯU PRODUCT (chưa có media) ===
                await _productRepo.AddAsync(newProduct);
                await _productRepo.SaveChangesAsync(); // STEP 1: Save Product + Variants (Lấy Id)

                // ===  GÁN ID VÀ LƯU MEDIA (THEO THIẾT KẾ CỦA BẠN) ===
                int variantIndex = 0;
                foreach (var media in mediaEntitiesToSave)
                {
                    if (media.EntityType == "product")
                    {
                        media.EntityId = newProduct.Id;
                    }
                    else if (media.EntityType == "variant")
                    {
                        // Gán media cho variant theo đúng thứ tự
                        // (Giả định: list Variants và list ảnh Variant trong DTO là 1-1)
                        if (variantIndex < newProduct.Variants.Count)
                        {
                            media.EntityId = newProduct.Variants.ElementAt(variantIndex).Id;
                            variantIndex++;
                        }
                    }
                }

                await _mediaRepo.AddRangeAsync(mediaEntitiesToSave);
                await _mediaRepo.SaveChangesAsync(); // STEP 2: Save Media

                // ===  COMMIT TRANSACTION ===
                await transaction.CommitAsync();

                // ===  Trả về DTO ===
                // (Vì đã xóa ICollection, chúng ta phải tự query media)
                // (Bỏ qua bước này, trả về DTO đơn giản trước)
                var responseDto = new CreateProductResponseDto
                {
                    Id = newProduct.Id,
                    Name = newProduct.Name,
                    ShopId = newProduct.ShopId,
                    Variants = newProduct.Variants.Select(v => new CreateVariantResponseDto
                    {
                        Id = v.Id,
                        SKU = v.SKU,
                        Price = v.Price,
                        Quantity = v.Quantity,
                        Material = v.Material,
                        ImageUrl = mediaEntitiesToSave.FirstOrDefault(m => m.EntityId == v.Id && m.EntityType == "variant")?.ImageUrl ?? ""
                    }).ToList(),
                    ProductImageUrl = mediaEntitiesToSave.FirstOrDefault(m => m.EntityId == newProduct.Id && m.EntityType == "product" && m.IsPrimary)?.ImageUrl ?? "",
                    VariantCount = newProduct.VariantCount
                };

                return new ApiResponseDTO<CreateProductResponseDto>
                {
                    IsSuccess = true,
                    Message = "Product created successfully",
                    Data = responseDto
                };
            }
            catch (Exception ex)
            {
                // === ROLLBACK (Cả DB và Cloudinary) ===
                await transaction.RollbackAsync();

                if (uploadedMediaResults.Any())
                {
                    foreach (var result in uploadedMediaResults)
                    {
                        await _mediaUploadService.DeleteImageAsync(result.PublicId);
                    }
                }
                return new ApiResponseDTO<CreateProductResponseDto>
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"An internal error occurred: {ex.InnerException?.Message ?? ex.ToString()}"
                };
            }
        }
        //
        //
        public async Task<ApiResponseDTO<List<ProductSummaryResponseDto>>> GetProductsForSellerAsync(int sellerId)
        {
            // 1. Phân quyền: Check xem user này có shop không
            var shop = await _shopRepo.GetByOwnerIdAsync(sellerId);
            if (shop == null)
            {
                return new ApiResponseDTO<List<ProductSummaryResponseDto>>
                {
                    IsSuccess = false,
                    Code = 403,
                    Message = "Người dùng không phải là seller"
                };
            }

            // 2. Lấy dữ liệu Product (đã tối ưu)
            var products = await _productRepo.GetProductsByShopIdAsync(shop.Id);
            if (!products.Any())
            {
                // Trả về danh sách rỗng (vẫn là Success)
                return new ApiResponseDTO<List<ProductSummaryResponseDto>> { IsSuccess = true, Data = new List<ProductSummaryResponseDto>() };
            }

            var productIds = products.Select(p => p.Id).ToList();

            // 3. Lấy ảnh đại diện (Chạy 1 query duy nhất)
            var primaryImages = await _mediaRepo.GetPrimaryMediaForEntitiesAsync(productIds, "product");

            // 4. Map (ánh xạ) sang DTO
            var dtos = products.Select(p => new ProductSummaryResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                MinPrice = p.MinPrice,
                VariantCount = p.VariantCount,
                Status = p.Status,
                SelledCount = 0,
                ReviewCount =0,
                AverageRating = 0,
                //SelledCount = product.SelledCount,
                //ReviewCount = product.ReviewCount,
                //AverageRating = product.AverageRating
                // Lấy ảnh từ Dictionary, nếu không có thì trả về null
                PrimaryImageUrl = primaryImages.GetValueOrDefault(p.Id, null),
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name
            }).ToList();

            return new ApiResponseDTO<List<ProductSummaryResponseDto>> { IsSuccess = true, Data = dtos };
        }
        //
        //
        public async Task<ApiResponseDTO<ProductDetailResponseDto>> GetProductDetailForSellerAsync(int productId, int sellerId)
        {
            // 1. Lấy dữ liệu "nặng" (Product, Variants, Shop, Category)
            var product = await _productRepo.GetProductDetailByIdAsync(productId);

            // 2. Check "Tồn tại"
            if (product == null)
            {
                return new ApiResponseDTO<ProductDetailResponseDto>
                { IsSuccess = false, Code = 404, Message = "Product not found" };
            }

            // 3. Check Phân quyền "Production-Ready"
            // (Check xem Shop.OwnerId có khớp với sellerId từ Token không)
            if (product.Shop.OwnerId != sellerId)
            {
                return new ApiResponseDTO<ProductDetailResponseDto>
                { IsSuccess = false, Code = 403, Message = "You do not have permission to view this product" };
            }

            // 4. Lấy dữ liệu "Media" (Chống N+1)
            // Lấy TẤT CẢ ảnh của "Cha" (Product)
            var productMedia = await _mediaRepo.GetMediaForEntityAsync(product.Id, "product");

            // Lấy ẢNH CHÍNH của TẤT CẢ "Con" (Variants)
            var variantIds = product.Variants.Select(v => v.Id).ToList();
            var variantPrimaryImages = await _mediaRepo.GetPrimaryMediaForEntitiesMapAsync(variantIds, "variant");

            // 5. "Map" (Ánh xạ) sang DTO "Chi tiết"
            var dto = new ProductDetailResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Brand = product.Brand,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name,
                ShopId = product.ShopId,
                Status = product.Status,

                ProductImages = productMedia.Select(m => new ProductMediaDto
                {
                    Id = m.Id,
                    ImageUrl = m.ImageUrl ?? "",
                    IsPrimary = m.IsPrimary
                }).ToList(),

                // === SỬA LỖI MAPPING (VARIANT) ===
                Variants = product.Variants.Select(v => {
                    // Lấy object Media từ Dictionary
                    var primaryMediaObject = variantPrimaryImages.GetValueOrDefault(v.Id);

                    return new ProductVariantDetailDto
                    {
                        Id = v.Id,
                        SKU = v.SKU,
                        VariantSize = v.VariantSize,
                        Color = v.Color,
                        Material = v.Material,
                        Price = v.Price,
                        Quantity = v.Quantity,
                        IsInStock = v.Quantity > 0,

                        // Ánh xạ sang DTO object (nếu có)
                        PrimaryImage = primaryMediaObject == null ? null : new VariantMediaDto
                        {
                            Id = primaryMediaObject.Id,
                            ImageUrl = primaryMediaObject.ImageUrl ?? ""
                        }
                    };
                }).ToList()
            };

            return new ApiResponseDTO<ProductDetailResponseDto> { IsSuccess = true, Data = dto };
        }
        //
        //
        public async Task<ApiResponseDTO<ProductDetailResponseDto>> GetProductDetailForCustomerAsync(int productId)
        {
            // 1. Lấy dữ liệu "nặng" (Product, Variants, Shop, Category)
            var product = await _productRepo.GetProductDetailByIdAsync(productId);

            // 2. Check "Tồn tại"
            if (product == null)
            {
                return new ApiResponseDTO<ProductDetailResponseDto>
                { IsSuccess = false, Code = 404, Message = "Product not found" };
            }
            // 4. Lấy dữ liệu "Media" (Chống N+1)
            // Lấy TẤT CẢ ảnh của "Cha" (Product)
            var productMedia = await _mediaRepo.GetMediaForEntityAsync(product.Id, "product");

            // Lấy ẢNH CHÍNH của TẤT CẢ "Con" (Variants)
            var variantIds = product.Variants.Select(v => v.Id).ToList();
            var variantPrimaryImages = await _mediaRepo.GetPrimaryMediaForEntitiesMapAsync(variantIds, "variant");

            // 5. "Map" (Ánh xạ) sang DTO "Chi tiết"
            var dto = new ProductDetailResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Brand = product.Brand,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name,
                ShopId = product.ShopId,
                Status = product.Status,

                ProductImages = productMedia.Select(m => new ProductMediaDto
                {
                    Id = m.Id,
                    ImageUrl = m.ImageUrl ?? "",
                    IsPrimary = m.IsPrimary
                }).ToList(),

                // === SỬA LỖI MAPPING (VARIANT) ===
                Variants = product.Variants.Select(v => {
                    // Lấy object Media từ Dictionary
                    var primaryMediaObject = variantPrimaryImages.GetValueOrDefault(v.Id);

                    return new ProductVariantDetailDto
                    {
                        Id = v.Id,
                        SKU = v.SKU,
                        VariantSize = v.VariantSize,
                        Color = v.Color,
                        Material = v.Material,
                        Price = v.Price,
                        Quantity = v.Quantity,
                        IsInStock = v.Quantity > 0,

                        // Ánh xạ sang DTO object (nếu có)
                        PrimaryImage = primaryMediaObject == null ? null : new VariantMediaDto
                        {
                            Id = primaryMediaObject.Id,
                            ImageUrl = primaryMediaObject.ImageUrl ?? ""
                        }
                    };
                }).ToList()
            };

            return new ApiResponseDTO<ProductDetailResponseDto> { IsSuccess = true, Data = dto };
        }
        //
        //
        public async Task<ApiResponseDTO<UpdateProductResponseDto>> UpdateProductAsync(int productId, UpdateProductRequestDto dto, int sellerId)
        {
            // === BƯỚC 1: VALIDATION (Check 404 - Not Found) ===

            // Sử dụng hàm "Get" nhẹ mới, KHÔNG load Variants
            var product = await _productRepo.GetProductForUpdateAsync(productId);

            if (product == null)
            {
                return new ApiResponseDTO<UpdateProductResponseDto>
                {
                    IsSuccess = false,
                    Code = 404,
                    Message = $"Product with ID {productId} not found."
                };
            }

            // === BƯỚC 2: VALIDATION (Check 403 - Quyền sở hữu) ===
            if (product.Shop.OwnerId != sellerId)
            {
                return new ApiResponseDTO<UpdateProductResponseDto>
                {
                    IsSuccess = false,
                    Code = 403,
                    Message = "Forbidden: You do not have permission to edit this product."
                };
            }

            // Biến tạm để lưu tên Category cho DTO phản hồi
            string categoryNameForResponse = product.Category.Name; // Mặc định là tên cũ

            // === BƯỚC 3: VALIDATION (Check 400 - CategoryId mới) ===
            if (dto.CategoryId != product.CategoryId) // Chỉ check DB khi CategoryId thay đổi
            {
                var categoryExists = await _categoryRepo.GetByIdAsync(dto.CategoryId);
                if (categoryExists == null)
                {
                    return new ApiResponseDTO<UpdateProductResponseDto>
                    {
                        IsSuccess = false,
                        Code = 400,
                        Message = $"New CategoryId '{dto.CategoryId}' does not exist."
                    };
                }

                // Cập nhật tên mới để trả về
                categoryNameForResponse = categoryExists.Name;
            }

            // === BƯỚC 4: THỰC THI UPDATE ===
            // Ánh xạ các trường từ DTO
            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Brand = dto.Brand;
            product.CategoryId = dto.CategoryId;
            product.UpdatedAt = DateTime.UtcNow;

            _productRepo.Update(product); // Đánh dấu "Sửa"
            await _productRepo.SaveChangesAsync(); // Lưu thay đổi

            // === BƯỚC 5: TRẢ VỀ DỮ LIỆU "NHẸ" MỚI ===
            var responseDto = new UpdateProductResponseDto
            {
                Id = product.Id,
                UpdatedAt = DateTime.UtcNow,
                CategoryName = categoryNameForResponse
            };

            return new ApiResponseDTO<UpdateProductResponseDto>
            {
                IsSuccess = true,
                Message = "Product updated successfully.",
                Data = responseDto
            };
        }
        //
        //
        // === UPDATE VARIANT (CHỨC NĂNG 4A) ===
        public async Task<ApiResponseDTO<UpdateProductVariantResponseDto>> UpdateProductVariantAsync(
            int productId, int variantId, UpdateProductVariantRequestDto dto, int sellerId)
        {
            // 1. Lấy Variant (đã bao gồm Product -> Shop)
            var variant = await _variantRepo.GetVariantWithProductAndShopAsync(variantId);

            // 2. Validation 404
            if (variant == null)
            {
                return new ApiResponseDTO<UpdateProductVariantResponseDto>
                { IsSuccess = false, Code = 404, Message = $"Variant with ID {variantId} not found." };
            }

            // 3. Validation 400 (Cha-Con)
            if (variant.ProductId != productId)
            {
                return new ApiResponseDTO<UpdateProductVariantResponseDto>
                { IsSuccess = false, Code = 400, Message = $"Variant {variantId} does not belong to Product {productId}." };
            }

            // 4. Validation 403 (Quyền sở hữu)
            if (variant.Product.Shop.OwnerId != sellerId)
            {
                return new ApiResponseDTO<UpdateProductVariantResponseDto>
                { IsSuccess = false, Code = 403, Message = "Forbidden: You do not have permission to edit this variant." };
            }

            // 5. Validation 400 (Thuộc tính Size/Color) - "Lưới an toàn"
            var validationError = VariantValidator.ValidateAttributes(dto.VariantSize, dto.Color);
            if (validationError != null)
            {
                return new ApiResponseDTO<UpdateProductVariantResponseDto>
                { IsSuccess = false, Code = 400, Message = validationError };
            }

            // 6. (TODO: Validation 400 (SKU bị trùng lặp với variant khác))
            // (Chúng ta sẽ thêm sau nếu cần)

            // 7. Thực thi Update
            variant.SKU = dto.SKU;
            variant.Price = dto.Price;
            variant.Quantity = dto.Quantity;
            variant.VariantSize = dto.VariantSize; // Đã validate
            variant.Color = dto.Color; // Đã validate
            variant.Material = dto.Material;
            variant.UpdatedAt = DateTime.UtcNow;

            _variantRepo.Update(variant);
            await _context.SaveChangesAsync();

            // 8. Trả về DTO "nhẹ" (CQRS)
            var responseDto = new UpdateProductVariantResponseDto
            {
                VariantId = variant.Id,
                ProductId = variant.ProductId,
                UpdatedAt = DateTime.UtcNow
            };

            return new ApiResponseDTO<UpdateProductVariantResponseDto>
            { IsSuccess = true, Message = "Variant updated successfully.", Data = responseDto };
        }
        //
        //
        // === CHỨC NĂNG 4B: THÊM VARIANT MỚI ===

        public async Task<ApiResponseDTO<ProductVariantDetailDto>> AddVariantAsync(
            int productId, AddVariantRequestDto dto, int sellerId)
        {
            // === 1. LẤY "CHA" (PRODUCT) ĐỂ VALIDATE ===
            var product = await _productRepo.GetProductForUpdateAsync(productId);

            // Validation 404 (Product)
            if (product == null)
            {
                return new ApiResponseDTO<ProductVariantDetailDto>
                { IsSuccess = false, Code = 404, Message = $"Product with ID {productId} not found." };
            }

            // Validation 403 (Quyền sở hữu)
            if (product.Shop.OwnerId != sellerId)
            {
                return new ApiResponseDTO<ProductVariantDetailDto>
                { IsSuccess = false, Code = 403, Message = "Forbidden: You do not have permission to add variants to this product." };
            }

            // === 2. VALIDATION DTO (SKU, Size/Color) ===
            if (await _variantRepo.IsSkuExistsAsync(dto.SKU))
            {
                return new ApiResponseDTO<ProductVariantDetailDto>
                { IsSuccess = false, Code = 400, Message = $"Bad Request: SKU '{dto.SKU}' already exists." };
            }

            var validationError = VariantValidator.ValidateAttributes(dto.VariantSize, dto.Color);
            if (validationError != null)
            {
                return new ApiResponseDTO<ProductVariantDetailDto>
                { IsSuccess = false, Code = 400, Message = validationError };
            }

            MediaUploadResult? uploadedMediaResult = null;
            await using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // === 3. UPLOAD ẢNH (Cloudinary) ===
                uploadedMediaResult = await _mediaUploadService.UploadImageAsync(dto.Image);

                // === 4. TẠO VARIANT VÀ MEDIA MỚI ===
                var newVariant = new ProductVariant
                {
                    ProductId = productId,
                    SKU = dto.SKU,
                    VariantSize = dto.VariantSize,
                    Color = dto.Color,
                    Material = dto.Material,
                    Price = dto.Price,
                    Quantity = dto.Quantity,
                    CreatedAt = DateTime.UtcNow
                };

                var newMedia = new Media
                {
                    EntityType = "variant",
                    ImageUrl = uploadedMediaResult.Url,
                    PublicId = uploadedMediaResult.PublicId,
                    IsPrimary = true,
                    CreatedAt = DateTime.UtcNow
                };

                // === 5. LƯU VARIANT (STEP 1) ===
                await _variantRepo.AddAsync(newVariant);
                await _variantRepo.SaveChangesAsync(); // (Lấy newVariant.Id)

                // === 6. LƯU MEDIA (STEP 2) ===
                newMedia.EntityId = newVariant.Id; // Gán ID (đã có)
                await _mediaRepo.AddRangeAsync(new List<Media> { newMedia });
                await _mediaRepo.SaveChangesAsync();

                // === 7. CẬP NHẬT PRODUCT (STEP 3) ===
                product.VariantCount++;
                if (dto.Price < product.MinPrice)
                {
                    product.MinPrice = dto.Price;
                }
                product.UpdatedAt = DateTime.UtcNow;

                _productRepo.Update(product);
                await _productRepo.SaveChangesAsync(); // (Lưu Product)

                // === 8. COMMIT TRANSACTION ===
                await transaction.CommitAsync();

                // === 9. TRẢ VỀ DTO "NẶNG" (PRAGMATIC RESPONSE) ===
                var responseDto = new ProductVariantDetailDto
                {
                    Id = newVariant.Id,
                    SKU = newVariant.SKU,
                    VariantSize = newVariant.VariantSize,
                    Color = newVariant.Color,
                    Material = newVariant.Material,
                    Price = newVariant.Price,
                    Quantity = newVariant.Quantity,
                    PrimaryImage = new VariantMediaDto
                    {
                        Id = newMedia.Id,
                        ImageUrl = newMedia.ImageUrl ?? ""
                    }
                };

                return new ApiResponseDTO<ProductVariantDetailDto>
                { IsSuccess = true, Message = "Variant added successfully", Data = responseDto };
            }
            catch (Exception ex)
            {
                // === ROLLBACK (Cả DB và Cloudinary) ===
                await transaction.RollbackAsync();
                if (uploadedMediaResult != null)
                {
                    await _mediaUploadService.DeleteImageAsync(uploadedMediaResult.PublicId);
                }

                return new ApiResponseDTO<ProductVariantDetailDto>
                { IsSuccess = false, Code = 500, Message = $"An internal error occurred: {ex.Message}" };
            }
        }
        //
        //
        // === CHỨC NĂNG 4C: XÓA VARIANT (XÓA CỨNG) ===

        public async Task<ApiResponseDTO<string>> DeleteVariantAsync(
            int productId, int variantId, int sellerId)
        {
            // === 1. VALIDATION (Lấy dữ liệu) ===
            // (Không dùng AsNoTracking() vì chúng ta sẽ xóa/sửa)
            var product = await _productRepo.GetProductForUpdateAsync(productId);
            var variant = await _variantRepo.GetByIdAsync(variantId); // (GetByIdAsync mới không dùng AsNoTracking)

            // Check 404 (Product)
            if (product == null)
            {
                return new ApiResponseDTO<string>
                { IsSuccess = false, Code = 404, Message = $"Product with ID {productId} not found." };
            }
            // Check 404 (Variant)
            if (variant == null)
            {
                return new ApiResponseDTO<string>
                { IsSuccess = false, Code = 404, Message = $"Variant with ID {variantId} not found." };
            }
            // Check 400 (Cha-Con)
            if (variant.ProductId != productId)
            {
                return new ApiResponseDTO<string>
                { IsSuccess = false, Code = 400, Message = $"Variant {variantId} does not belong to Product {productId}." };
            }
            // Check 403 (Quyền sở hữu)
            if (product.Shop.OwnerId != sellerId)
            {
                return new ApiResponseDTO<string>
                { IsSuccess = false, Code = 403, Message = "Forbidden: You do not have permission to delete this variant." };
            }

            // === 2. BẮT ĐẦU TRANSACTION ===
            // (Rất quan trọng vì chúng ta xóa/sửa nhiều bảng + Cloudinary)
            await using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();

            // List để rollback Cloudinary (nếu DB lỗi)
            List<Media> mediaToDelete = new List<Media>();
            string successMessage;

            try
            {
                // === 3. LOGIC NGHIỆP VỤ (QUY TẮC 1) ===
                var variantCount = await _variantRepo.CountByProductIdAsync(productId);

                if (variantCount > 1)
                {
                    // === CASE 1: XÓA 1 VARIANT (Không phải cái cuối) ===

                    // a. Lấy media của Variant
                    mediaToDelete = (await _mediaRepo.GetMediaForEntityAsync(variantId, "variant")).ToList();

                    // b. Xóa Variant + Media (DB)
                    _mediaRepo.DeleteRange(mediaToDelete);
                    _variantRepo.Delete(variant);

                    // c. Cập nhật "Cha" (Product)
                    product.VariantCount--;
                    var remainingVariants = await _variantRepo.GetVariantsByProductIdAsync(productId);

                    // Tính lại MinPrice (bỏ qua variant vừa xóa)
                    product.MinPrice = remainingVariants
                        .Where(v => v.Id != variantId)
                        .DefaultIfEmpty() // Tránh lỗi nếu list rỗng
                        .Min(v => (v?.Price ?? 0));

                    _productRepo.Update(product);

                    // d. Lưu DB (Step 1)
                    await _context.SaveChangesAsync();

                    // e. Xóa Cloudinary (Step 2)
                    foreach (var media in mediaToDelete)
                    {
                        if (media.PublicId != null)
                            await _mediaUploadService.DeleteImageAsync(media.PublicId);
                    }

                    successMessage = "Variant deleted successfully.";
                }
                else
                {
                    // === CASE 2: XÓA VARIANT CUỐI (Tự động xóa Product) ===

                    // a. Lấy TẤT CẢ media (của Cha VÀ Con)
                    var productMedia = await _mediaRepo.GetMediaForEntityAsync(productId, "product");
                    var variantMedia = await _mediaRepo.GetMediaForEntityAsync(variantId, "variant");
                    mediaToDelete = productMedia.Concat(variantMedia).ToList();

                    // b. Xóa Product (DB) -> Sẽ CASCADE xóa Variant
                    // (Chúng ta KHÔNG cần gọi _variantRepo.Delete(variant))
                    _productRepo.Delete(product);

                    // c. Xóa Media (DB)
                    _mediaRepo.DeleteRange(mediaToDelete);

                    // d. Lưu DB (Step 1)
                    await _context.SaveChangesAsync();

                    

                    successMessage = "Last variant was deleted, which triggered deletion of the parent product.";
                }

                // === 4. COMMIT TRANSACTION ===
                await transaction.CommitAsync();
                // e. Xóa Cloudinary (Step 2)

            }
               
            catch (Exception ex)
            {
                // === ROLLBACK (Chỉ DB) ===
                await transaction.RollbackAsync();
                // (Chúng ta không rollback Cloudinary, vì nếu DB lỗi,
                // chúng ta thà để "rác" trên Cloudinary còn hơn là mất ảnh)

                return new ApiResponseDTO<string>
                { IsSuccess = false, Code = 500, Message = $"An internal error occurred: {ex.Message}" };
            }
            try
            {
                // (Chạy song song để tăng tốc)
                var deleteTasks = mediaToDelete
                    .Where(m => !string.IsNullOrEmpty(m.PublicId))
                    .Select(m => _mediaUploadService.DeleteImageAsync(m.PublicId!));
                await Task.WhenAll(deleteTasks);
            }
            catch (Exception cloudEx)
            {
                // KHÔNG TRẢ VỀ LỖI 500 CHO USER
                // User đã XÓA thành công. Đây là lỗi dọn dẹp "mồ côi".
                _logger.LogError(cloudEx,
                    "THÀNH CÔNG (DB) nhưng LỖI (Cloudinary): Không dọn dẹp được ảnh mồ côi của ProductID {ProductId}", productId);

                // (Chúng ta vẫn trả về 200 OK, nhưng báo cho Admin/Dev biết)
                return new ApiResponseDTO<string>
                {
                    IsSuccess = true,
                    Message = "Product deleted from database, but failed to clean up some images on the storage server. Please contact support."
                };
            }
            return new ApiResponseDTO<string> { IsSuccess = true, Message = successMessage };

        }
        //
        //
        // === CHỨC NĂNG MỚI: XÓA PRODUCT ===

        public async Task<ApiResponseDTO<string>> DeleteProductAsync(int productId, int sellerId)
        {
            // === 1. VALIDATION (Check 404, 403) ===
            // (Dùng GetProductForUpdateAsync để EF theo dõi Product)
            var product = await _productRepo.GetProductForUpdateAsync(productId);

            if (product == null)
            {
                return new ApiResponseDTO<string>
                { IsSuccess = false, Code = 404, Message = $"Product with ID {productId} not found." };
            }

            if (product.Shop.OwnerId != sellerId)
            {
                return new ApiResponseDTO<string>
                { IsSuccess = false, Code = 403, Message = "Forbidden: You do not have permission to delete this product." };
            }

            // === 2. LẤY DỮ LIỆU CẦN XÓA (ẢNH) ===
            // (Phải lấy TRƯỚC khi xóa DB, để giữ lại PublicId)

            // a. Lấy ảnh Product (cha)
            var productMedia = await _mediaRepo.GetMediaForEntityAsync(productId, "product");

            // b. Lấy ảnh Variant (con)
            var variants = await _variantRepo.GetVariantsByProductIdAsync(productId);
            var variantIds = variants.Select(v => v.Id).ToList();
            var variantMediaDict = await _mediaRepo.GetPrimaryMediaForEntitiesMapAsync(variantIds, "variant");

            // c. Gộp chung
            var allMediaToDelete = new List<Media>(productMedia);
            allMediaToDelete.AddRange(variantMediaDict.Values);

            // === 3. BẮT ĐẦU TRANSACTION (DB TRƯỚC) ===
            await using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // a. Xóa Media (cháu)
                _mediaRepo.DeleteRange(allMediaToDelete);

                // b. Xóa Product (cha)
                _productRepo.Delete(product);

                // c. (DATABASE) Tự động CASCADE Xóa Variants (con)
                // (Như bạn đã nói, DB đã cài đặt ON DELETE CASCADE
                // từ Product -> ProductVariant)

                // d. Lưu DB
                await _context.SaveChangesAsync();

                // e. COMMIT! (Chốt DB)
                await transaction.CommitAsync();
            }
            catch (Exception dbEx)
            {
                // === ROLLBACK DB ===
                await transaction.RollbackAsync();
                _logger.LogError(dbEx, "Lỗi DB khi xóa ProductID {ProductId}", productId);
                return new ApiResponseDTO<string>
                { IsSuccess = false, Code = 500, Message = $"An internal database error occurred: {dbEx.Message}" };
            }

            // === 4. DỌN DẸP CLOUDINARY (SAU KHI DB THÀNH CÔNG) ===
            // (Theo logic "An toàn tuyệt đối")

            try
            {
                // (Chạy song song để tăng tốc)
                var deleteTasks = allMediaToDelete
                    .Where(m => !string.IsNullOrEmpty(m.PublicId))
                    .Select(m => _mediaUploadService.DeleteImageAsync(m.PublicId!));

                await Task.WhenAll(deleteTasks);
            }
            catch (Exception cloudEx)
            {
                // KHÔNG TRẢ VỀ LỖI 500 CHO USER
                // User đã XÓA thành công. Đây là lỗi dọn dẹp "mồ côi".
                _logger.LogError(cloudEx,
                    "THÀNH CÔNG (DB) nhưng LỖI (Cloudinary): Không dọn dẹp được ảnh mồ côi của ProductID {ProductId}", productId);

                // (Chúng ta vẫn trả về 200 OK, nhưng báo cho Admin/Dev biết)
                return new ApiResponseDTO<string>
                {
                    IsSuccess = true,
                    Message = "Product deleted from database, but failed to clean up some images on the storage server. Please contact support."
                };
            }

            // === 5. HOÀN TẤT ===
            return new ApiResponseDTO<string> { IsSuccess = true, Message = "Product and all its variants/media were deleted successfully." };
        }
        //
        //
        /// <summary>
        /// (SỬA ĐỔI) Lấy các sản phẩm liên quan (CHỈ CÙNG SHOP)
        /// </summary>
        public async Task<ApiResponseDTO<RelatedProductsResponseDto>> GetRelatedProductsAsync(int productId)
        {
            try
            {
                // 1. Lấy sản phẩm gốc để biết ShopId và CategoryId
                var product = await _productRepo.GetProductEntityByIdAsync(productId); // Cần hàm này
                if (product == null)
                {
                    return new ApiResponseDTO<RelatedProductsResponseDto> { IsSuccess = false, Code = 404, Message = "Không tìm thấy sản phẩm." };
                }

                // 2. Gọi Repository 1 LẦN (chỉ lấy cùng Shop)
                var sameShopProducts = await _productRepo.GetRelatedProductsAsCardsAsync(
                    product.ShopId,
                    // null, // Đã bỏ categoryId
                    productId,5
                );

                // Chờ cả 2 query chạy xong

                // 3. Đóng gói DTO trả về
                var responseDto = new RelatedProductsResponseDto
                {
                    SameShopProducts = sameShopProducts
                    // ĐÃ XÓA: SameCategoryProducts = await sameCategoryTask
                };

                return new ApiResponseDTO<RelatedProductsResponseDto>
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Lấy sản phẩm tương tự thành công.",
                    Data = responseDto
                };
            }
            catch (Exception ex)
            {
                // (Log lỗi)
                return new ApiResponseDTO<RelatedProductsResponseDto> { IsSuccess = false, Code = 500, Message = ex.Message };
            }
        }
        //
        //
        public async Task<ApiResponseDTO<PagedListResponseDto<ProductCardDto>>> GetProductListForCustomerAsync(ProductListQueryRequestDto query)
        {
          
            try
            {
                // Giao hết công việc Query "nặng" cho Repository
                // Repo sẽ tự Lọc, Sắp xếp, Phân trang, và Ánh xạ sang DTO
                var pagedResult = await _productRepo.GetPaginatedProductCardsAsync(query);

                return new ApiResponseDTO<PagedListResponseDto<ProductCardDto>>
                {
                    IsSuccess = true,
                    Code=200,
                    Message = "Lấy danh sách sản phẩm thành công.",
                    Data = pagedResult
                };
            }
            catch (Exception ex)
            {
                // (Log lỗi ở đây)
                Debug.WriteLine($"Lỗi khi lấy danh sách sản phẩm: {ex.Message}");
                return new ApiResponseDTO<PagedListResponseDto<ProductCardDto>>
                {
                    IsSuccess = false,
                    Code = 500, 
                    Message = "Lỗi hệ thống khi lấy danh sách sản phẩm."
                };
            }
        }
       
    }
}


