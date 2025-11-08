using BackendEcommerce.Infrastructure.Persistence.Models;

namespace BackendEcommerce.Domain.Contracts.Persistence
{
    public interface IReviewRepository
    {
        Task<IReadOnlyList<Review>> GetReviewsForProductAsync(int productId);
    }
}
