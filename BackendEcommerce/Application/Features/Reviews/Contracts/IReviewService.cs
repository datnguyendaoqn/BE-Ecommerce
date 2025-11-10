using BackendEcommerce.Application.Features.Reviews.DTOs;
using BackendEcommerce.Application.Shared.DTOs;

namespace BackendEcommerce.Application.Features.Reviews.Contracts
{
    public interface IReviewService
    {
        Task<ApiResponseDTO<List<ReviewProductResponseDto>>> GetProductReviewsAsync(int productId);
    }
}
