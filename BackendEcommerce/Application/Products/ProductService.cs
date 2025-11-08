using BackendEcommerce.Application.Products.DTOs;
using BackendEcommerce.Application.Shared.DTOs;
using BackendEcommerce.Domain.Contracts.Persistence;
using BackendEcommerce.Domain.Contracts.Services;
using BackendEcommerce.Infrastructure.Persistence.Data;
using BackendEcommerce.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore.Storage;
namespace BackendEcommerce.Application.Products
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepo;
        private readonly IShopRepository _shopRepo;
        private readonly IMediaRepository _mediaRepo;
        private readonly IMediaUploadService _mediaUploadService;
        private readonly ICategoryRepository _categoryRepo;
        private readonly EcomDbContext _context; // <-- Thêm (chỉ để dùng Transaction)

        public ProductService(
            IProductRepository productRepo,
            IShopRepository shopRepo,
            IMediaRepository mediaRepo,
            IMediaUploadService mediaUploadService,
            ICategoryRepository categoryRepo,
            EcomDbContext context)
        {
            _productRepo = productRepo;
            _shopRepo = shopRepo;
            _mediaRepo = mediaRepo;
            _mediaUploadService = mediaUploadService;
            _categoryRepo = categoryRepo;
            _context = context;
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
                            IsPrimary = (productImageIndex == 0), // Ảnh đầu tiên là ảnh chính
                            CreatedAt = DateTime.UtcNow
                        });
                        productImageIndex++;
                    }
                // ===  Upload và Chuẩn bị Media cho Variant (Level) ===
                foreach (var variantDto in dto.Variants)
                {
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
                        ImageUrl = mediaEntitiesToSave.FirstOrDefault(m => m.EntityId == v.Id && m.EntityType == "variant")?.ImageUrl ?? ""
                    }).ToList(),
                    ProductImageUrl = mediaEntitiesToSave.FirstOrDefault(m => m.EntityId == newProduct.Id && m.EntityType == "product"&& m.IsPrimary)?.ImageUrl ?? "",
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
                    Message = $"An internal error occurred: {ex.Message}"
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

                // Lấy ảnh Chính (IsPrimary=true) từ list media của Product
                PrimaryImageUrl = productMedia
                    .FirstOrDefault(m => m.IsPrimary)?.ImageUrl,

                // Lấy ảnh Gallery (IsPrimary=false) từ list media của Product
                GalleryImageUrls = productMedia
                    .Select(m => m.ImageUrl ?? "") // ?? "" để tránh null
                    .ToList(),

                Variants = product.Variants.Select(v => new ProductVariantDetailDto
                {
                    Id = v.Id,
                    SKU = v.SKU,
                    VariantSize = v.VariantSize,
                    Color = v.Color,
                    Material = v.Material,
                    Price = v.Price,
                    Quantity = v.Quantity,

                    // Lấy ảnh chính của Variant từ Dictionary
                    PrimaryImageUrl = variantPrimaryImages.GetValueOrDefault(v.Id)?.ImageUrl
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
    }
}


