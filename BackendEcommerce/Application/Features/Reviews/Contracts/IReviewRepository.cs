using BackendEcommerce.Infrastructure.Persistence.Models;

namespace BackendEcommerce.Application.Features.Reviews.Contracts
{
    public interface IReviewRepository
    {
        Task<IReadOnlyList<Review>> GetReviewsForProductAsync(int productId);
    }
}
