using BackendEcommerce.Application.Reviews.DTOs;
using BackendEcommerce.Application.Shared.DTOs;

namespace BackendEcommerce.Application.Reviews
{
    public interface IReviewService
    {
        Task<ApiResponseDTO<List<ReviewProductResponseDto>>> GetProductReviewsAsync(int productId);
    }
}
