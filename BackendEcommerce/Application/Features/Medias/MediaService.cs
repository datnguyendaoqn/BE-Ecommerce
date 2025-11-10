using BackendEcommerce.Application.Features.Medias.Contracts;
using BackendEcommerce.Application.Features.Medias.DTOs;
using BackendEcommerce.Application.Features.Products.Contracts;
using BackendEcommerce.Application.Shared.Contracts;
using BackendEcommerce.Application.Shared.DTOs;
using BackendEcommerce.Infrastructure.Persistence.Models;

namespace BackendEcommerce.Application.Features.Medias
{
    public class MediaService : IMediaService
    {
        private readonly IMediaRepository _mediaRepo;
        private readonly IProductRepository _productRepo; // Cần để check quyền 403
        private readonly IMediaUploadService _mediaUploadService;
        private readonly IProductVariantRepository _variantRepo;

        public MediaService(
             IMediaRepository mediaRepo,
             IProductRepository productRepo,
             IMediaUploadService mediaUploadService,
             IProductVariantRepository variantRepo) // <-- Tiêm (Inject)
        {
            _mediaRepo = mediaRepo;
            _productRepo = productRepo;
            _mediaUploadService = mediaUploadService;
            _variantRepo = variantRepo; // <-- Gán
        }

        public async Task<ApiResponseDTO<SetPrimaryMediaResponseDto>> SetProductPrimaryMediaAsync(
            int productId, int mediaId, int sellerId)
        {
            // === 1. VALIDATION (Check 403 - Quyền sở hữu) ===
            // Dùng hàm "Get" nhẹ (chỉ load Shop) để check quyền
            var product = await _productRepo.GetProductForUpdateAsync(productId);

            if (product == null)
            {
                return new ApiResponseDTO<SetPrimaryMediaResponseDto>
                {
                    IsSuccess = false,
                    Code = 404,
                    Message = $"Product with ID {productId} not found."
                };
            }

            if (product.Shop.OwnerId != sellerId)
            {
                return new ApiResponseDTO<SetPrimaryMediaResponseDto>
                {
                    IsSuccess = false,
                    Code = 403,
                    Message = "Forbidden: You do not have permission to modify this product."
                };
            }

            // === 2. VALIDATION (Check Media) ===
            var newPrimaryMedia = await _mediaRepo.GetByIdAsync(mediaId);

            if (newPrimaryMedia == null)
            {
                return new ApiResponseDTO<SetPrimaryMediaResponseDto>
                {
                    IsSuccess = false,
                    Code = 404,
                    Message = $"Media with ID {mediaId} not found."
                };
            }

            // Check xem Media này có đúng là của Product này không
            if (newPrimaryMedia.EntityId != productId || newPrimaryMedia.EntityType != "product")
            {
                return new ApiResponseDTO<SetPrimaryMediaResponseDto>
                {
                    IsSuccess = false,
                    Code = 400,
                    Message = "Bad Request: This media item does not belong to the specified product."
                };
            }

            // Check xem nó có "đã là" primary rồi không
            if (newPrimaryMedia.IsPrimary)
            {
                return new ApiResponseDTO<SetPrimaryMediaResponseDto>
                {
                    IsSuccess = false,
                    Code = 400,
                    Message = "Bad Request: This media item is already the primary image."
                };
            }

            // === 3. EXECUTION (Logic "Hoán đổi") ===

            // Tìm ảnh chính "cũ"
            var allProductMedia = await _mediaRepo.GetMediaForEntityAsync(productId, "product");
            var currentPrimaryMedia = allProductMedia.FirstOrDefault(m => m.IsPrimary);

            if (currentPrimaryMedia != null)
            {
                // Tắt "cũ"
                currentPrimaryMedia.IsPrimary = false;
                _mediaRepo.Update(currentPrimaryMedia);
            }

            // Bật "mới"
            newPrimaryMedia.IsPrimary = true;
            _mediaRepo.Update(newPrimaryMedia);

            // Lưu cả 2 thay đổi trong 1 Giao dịch (Transaction)
            await _mediaRepo.SaveChangesAsync();

            // === 4. RESPONSE (Trả về DTO "nhẹ") ===
            var responseDto = new SetPrimaryMediaResponseDto
            {
                ProductId = productId,
                NewPrimaryMediaId = mediaId
            };

            return new ApiResponseDTO<SetPrimaryMediaResponseDto>
            {
                IsSuccess = true,
                Data = responseDto
            };
        }
        //
        //
        public async Task<ApiResponseDTO<AddGalleryImagesResponseDto>> AddGalleryImagesAsync(
           int productId, List<IFormFile> images, int sellerId)
        {
            // === 1. VALIDATION (Check 403 - Quyền sở hữu) ===
            var product = await _productRepo.GetProductForUpdateAsync(productId);

            if (product == null)
            {
                return new ApiResponseDTO<AddGalleryImagesResponseDto>
                {
                    IsSuccess = false,
                    Code = 404,
                    Message = $"Product with ID {productId} not found."
                };
            }

            if (product.Shop.OwnerId != sellerId)
            {
                return new ApiResponseDTO<AddGalleryImagesResponseDto>
                {
                    IsSuccess = false,
                    Code = 403,
                    Message = "Forbidden: You do not have permission to modify this product."
                };
            }

            // === 2. VALIDATION (Check 400 - Input) ===
            if (images == null || !images.Any())
            {
                return new ApiResponseDTO<AddGalleryImagesResponseDto>
                {
                    IsSuccess = false,
                    Code = 400,
                    Message = "Bad Request: No images provided for upload."
                };
            }

            // Chuẩn bị cho Rollback (nếu DB lỗi) và Response
            var uploadedMediaResults = new List<MediaUploadResult>();
            var mediaEntitiesToSave = new List<Media>();

            try
            {
                // === 3. UPLOAD LÊN CLOUDINARY ===
                foreach (var file in images)
                {
                    var uploadResult = await _mediaUploadService.UploadImageAsync(file);
                    uploadedMediaResults.Add(uploadResult); // Thêm vào list rollback

                    // Tạo Entity (chưa có ID)
                    var newMedia = new Media
                    {
                        EntityType = "product",
                        EntityId = productId,
                        ImageUrl = uploadResult.Url,
                        PublicId = uploadResult.PublicId,
                        IsPrimary = false, // <-- Quan trọng: Thêm gallery luôn là false
                        CreatedAt = DateTime.UtcNow
                    };
                    mediaEntitiesToSave.Add(newMedia);
                }

                // === 4. LƯU VÀO DATABASE ===
                await _mediaRepo.AddRangeAsync(mediaEntitiesToSave);
                // Giao dịch (Transaction) ngầm định của EF Core
                // sẽ bọc 1 lần SaveChanges này
                await _mediaRepo.SaveChangesAsync();

                // === 5. RESPONSE (Pragmatic DTO) ===
                // Giờ các 'mediaEntitiesToSave' đã có ID từ DB
                var responseDtos = mediaEntitiesToSave.Select(m => new ProductMediaDto
                {
                    Id = m.Id,
                    ImageUrl = m.ImageUrl ?? "",
                    IsPrimary = m.IsPrimary
                }).ToList();

                return new ApiResponseDTO<AddGalleryImagesResponseDto>
                {
                    IsSuccess = true,
                    Data = new AddGalleryImagesResponseDto { AddedImages = responseDtos }
                };
            }
            catch (Exception ex)
            {
                // === 6. ROLLBACK (Cloudinary) ===
                // Nếu bước 4 (Lưu DB) thất bại, chúng ta phải xóa
                // các ảnh đã upload ở bước 3.
                foreach (var result in uploadedMediaResults)
                {
                    await _mediaUploadService.DeleteImageAsync(result.PublicId);
                }

                return new ApiResponseDTO<AddGalleryImagesResponseDto>
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"An internal error occurred: {ex.Message}"
                };
            }
        }
        //
        //
        public async Task<ApiResponseDTO<string>> DeleteMediaAsync(int mediaId, int sellerId)
        {
            // === 1. VALIDATION (Check 404 - Media) ===
            var media = await _mediaRepo.GetByIdAsync(mediaId);
            if (media == null)
            {
                return new ApiResponseDTO<string>
                {
                    IsSuccess = false,
                    Code = 404,
                    Message = $"Media không tồn tại ."
                };
            }

            // === 2. VALIDATION (Check 400 - Cấm xóa Primary) ===
            if (media.IsPrimary && media.EntityType == "product")
            {
                return new ApiResponseDTO<string>
                {
                    IsSuccess = false,
                    Code = 400,
                    Message = "Không thể xoá ảnh hiển thị chính của product."
                };
            }

            // === 3. VALIDATION (Check 403 - Quyền sở hữu) ===
            if (media.EntityType == "product" || media.EntityType == "variant")
            {
                bool hasPermission = false;

                if (media.EntityType == "product")
                {
                    var product = await _productRepo.GetProductForUpdateAsync(media.EntityId ?? 0);
                    if (product != null && product.Shop.OwnerId == sellerId)
                    {
                        hasPermission = true;
                    }
                }
                else if (media.EntityType == "variant")
                {
                    // Dùng Repo mới để lấy Con (Variant) -> Cha (Product) -> Ông (Shop)
                    var variant = await _variantRepo.GetVariantWithProductAndShopAsync(media.EntityId ?? 0);

                    // Check xem Variant có tồn tại không VÀ
                    // SellerId có khớp với OwnerId của Shop (ông) không
                    if (variant != null && variant.Product.Shop.OwnerId == sellerId)
                    {
                        hasPermission = true;
                    }
                }

                if (!hasPermission)
                {
                    return new ApiResponseDTO<string>
                    {
                        IsSuccess = false,
                        Code = 403,
                        Message = "User không có quyền ."
                    };
                }
            }

            try
            {
                // === 4. EXECUTION (Xóa Cloudinary) ===
                if (!string.IsNullOrEmpty(media.PublicId))
                {
                    await _mediaUploadService.DeleteImageAsync(media.PublicId);
                }

                // === 5. EXECUTION (Xóa DB) ===
                _mediaRepo.Delete(media);
                await _mediaRepo.SaveChangesAsync(); // Transaction ngầm định

                return new ApiResponseDTO<string>
                {
                    IsSuccess = true,
                    Message = $"Media đã xoá thành công."
                };
            }
            catch (Exception ex)
            {
                // Lỗi này thường xảy ra nếu xóa Cloudinary thất bại
                // (Lưu ý: Nếu Cloudinary lỗi, DB sẽ không bị xóa)
                return new ApiResponseDTO<string>
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }
    }
}
