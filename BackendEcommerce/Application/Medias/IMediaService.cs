using BackendEcommerce.Application.Medias.DTOs;
using BackendEcommerce.Application.Shared.DTOs;

namespace BackendEcommerce.Application.Medias
{
    public interface IMediaService
    {
        Task<ApiResponseDTO<SetPrimaryMediaResponseDto>> SetProductPrimaryMediaAsync(int productId, int mediaId, int sellerId);
    }
}
