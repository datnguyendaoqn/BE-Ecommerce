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
        private readonly EcomDbContext _context; // <-- Thêm (chỉ để dùng Transaction)

        public ProductService(
            IProductRepository productRepo,
            IShopRepository shopRepo,
            IMediaRepository mediaRepo,
            IMediaUploadService mediaUploadService,
            EcomDbContext context)
        {
            _productRepo = productRepo;
            _shopRepo = shopRepo;
            _mediaRepo = mediaRepo;
            _mediaUploadService = mediaUploadService;
            _context = context;
        }

        public async Task<ApiResponseDTO<ProductResponseDto>> CreateProductAsync(CreateProductRequestDto dto, int sellerId)
        {
            // === 1. PHÂN QUYỀN (Authorization Check) ===
            var shop = await _shopRepo.GetByOwnerIdAsync(sellerId);
            if (shop == null)
            {
                return new ApiResponseDTO<ProductResponseDto>
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
                return new ApiResponseDTO<ProductResponseDto>
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
                return new ApiResponseDTO<ProductResponseDto>
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
                // === 2. Tạo Product Entity ===
                var newProduct = new Product
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    Brand = dto.Brand,
                    CategoryId = dto.CategoryId,
                    ShopId = shop.Id,
                    CreatedAt = DateTime.UtcNow,
                    VariantCount = dto.Variants.Count,
                    MinPrice = dto.Variants.Min(v => v.Price)
                };

                // === 3. Upload và Chuẩn bị Media cho Product (Level) ===
                if (dto.ProductImages != null)
                {
                        var uploadResult = await _mediaUploadService.UploadImageAsync(dto.ProductImages);
                        uploadedMediaResults.Add(uploadResult); // Thêm vào list rollback
                        mediaEntitiesToSave.Add(new Media
                        {
                            EntityType = "product",
                            // EntityId sẽ được gán sau khi SaveChanges() Product
                            ImageUrl = uploadResult.Url,
                            PublicId = uploadResult.PublicId,
                            IsPrimary = true,
                            CreatedAt = DateTime.UtcNow
                        });
                }

                // === 4. Upload và Chuẩn bị Media cho Variant (Level) ===
                foreach (var variantDto in dto.Variants)
                {
                    // 4a. Upload ảnh
                    var variantUploadResult = await _mediaUploadService.UploadImageAsync(variantDto.Image);
                    uploadedMediaResults.Add(variantUploadResult); // Thêm vào list rollback
                    // 4b. Tạo Variant Entity
                    var newVariant = new ProductVariant
                    {
                        SKU = variantDto.SKU,
                        VariantSize = variantDto.VariantSize,
                        Color = variantDto.Color,
                        Price = variantDto.Price,
                        Quantity = variantDto.Quantity,
                        CreatedAt = DateTime.UtcNow
                    };
                    // 4c. Thêm Variant vào Product
                    newProduct.Variants.Add(newVariant);
                    // 4d. Chuẩn bị Media cho Variant (chưa gán Id)
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

                // === 5. LƯU PRODUCT (chưa có media) ===
                await _productRepo.AddAsync(newProduct);
                await _productRepo.SaveChangesAsync(); // STEP 1: Save Product + Variants (Lấy Id)

                // === 6. GÁN ID VÀ LƯU MEDIA (THEO THIẾT KẾ CỦA BẠN) ===
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

                // === 7. COMMIT TRANSACTION ===
                await transaction.CommitAsync();

                // === 8. Trả về DTO ===
                // (Vì đã xóa ICollection, chúng ta phải tự query media)
                // (Bỏ qua bước này, trả về DTO đơn giản trước)
                var responseDto = new ProductResponseDto
                {
                    Id = newProduct.Id,
                    Name = newProduct.Name,
                    ShopId = newProduct.ShopId,
                    Variants = newProduct.Variants.Select(v => new VariantResponseDto
                    {
                        Id = v.Id,
                        SKU = v.SKU,
                        Price = v.Price,
                        Quantity = v.Quantity,
                        ImageUrl = mediaEntitiesToSave.FirstOrDefault(m => m.EntityId == v.Id && m.EntityType == "variant")?.ImageUrl ?? ""
                    }).ToList(),
                    ProductImageUrl = mediaEntitiesToSave.FirstOrDefault(m => m.EntityId == newProduct.Id && m.EntityType == "product")?.ImageUrl ?? "",
                    VariantCount = newProduct.VariantCount
                };

                return new ApiResponseDTO<ProductResponseDto>
                {
                    IsSuccess = true,
                    Message = "Product created successfully",
                    Data = responseDto
                };
            }
            catch (Exception ex)
            {
                // === 9. ROLLBACK (Cả DB và Cloudinary) ===
                await transaction.RollbackAsync();

                if (uploadedMediaResults.Any())
                {
                    foreach (var result in uploadedMediaResults)
                    {
                        await _mediaUploadService.DeleteImageAsync(result.PublicId);
                    }
                }
                return new ApiResponseDTO<ProductResponseDto>
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"An internal error occurred: {ex.Message}"
                };
            }
        }
        //
        //
        public async Task<ApiResponseDTO<List<ProductSummaryDto>>> GetProductsForSellerAsync(int sellerId)
        {
            // 1. Phân quyền: Check xem user này có shop không
            var shop = await _shopRepo.GetByOwnerIdAsync(sellerId);
            if (shop == null)
            {
                return new ApiResponseDTO<List<ProductSummaryDto>>
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
                return new ApiResponseDTO<List<ProductSummaryDto>> { IsSuccess = true, Data = new List<ProductSummaryDto>() };
            }

            var productIds = products.Select(p => p.Id).ToList();

            // 3. Lấy ảnh đại diện (Chạy 1 query duy nhất)
            var primaryImages = await _mediaRepo.GetPrimaryMediaForEntitiesAsync(productIds, "product");

            // 4. Map (ánh xạ) sang DTO
            var dtos = products.Select(p => new ProductSummaryDto
            {
                Id = p.Id,
                Name = p.Name,
                MinPrice = p.MinPrice,
                VariantCount = p.VariantCount,
                Status = p.Status,
                // Lấy ảnh từ Dictionary, nếu không có thì trả về null
                PrimaryImageUrl = primaryImages.GetValueOrDefault(p.Id, null)
            }).ToList();

            return new ApiResponseDTO<List<ProductSummaryDto>> { IsSuccess = true, Data = dtos };
        }
    }
}


