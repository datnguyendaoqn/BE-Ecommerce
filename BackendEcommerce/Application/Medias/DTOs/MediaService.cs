using BackendEcommerce.Application.Shared.DTOs;
using BackendEcommerce.Domain.Contracts.Persistence;

namespace BackendEcommerce.Application.Medias.DTOs
{
    public class MediaService : IMediaService
    {
        private readonly IMediaRepository _mediaRepo;
        private readonly IProductRepository _productRepo; // Cần để check quyền 403

        public MediaService(IMediaRepository mediaRepo, IProductRepository productRepo)
        {
            _mediaRepo = mediaRepo;
            _productRepo = productRepo;
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
    }
}
