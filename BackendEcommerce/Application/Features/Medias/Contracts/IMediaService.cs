using BackendEcommerce.Application.Features.Medias.DTOs;
using BackendEcommerce.Application.Shared.DTOs;

namespace BackendEcommerce.Application.Features.Medias.Contracts
{
    public interface IMediaService
    {
        Task<ApiResponseDTO<SetPrimaryMediaResponseDto>> SetProductPrimaryMediaAsync(int productId, int mediaId, int sellerId);
        Task<ApiResponseDTO<AddGalleryImagesResponseDto>> AddGalleryImagesAsync(int productId, List<IFormFile> images, int sellerId);
        Task<ApiResponseDTO<string>> DeleteMediaAsync(int mediaId, int sellerId);
    }
}
