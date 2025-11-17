using BackendEcommerce.Infrastructure.Persistence.Models;

namespace BackendEcommerce.Application.Features.Reviews.Contracts
{
    public interface IReviewRepository
    {
        Task<IReadOnlyList<Review>> GetReviewsForProductAsync(int productId);
        /// <summary>
        /// Kiểm tra xem OrderItemId này đã được review chưa
        /// </summary>
        Task<bool> HasAlreadyReviewedAsync(int orderItemId);

        /// <summary>
        /// Thêm Review (theo chuẩn chung Repo)
        /// </summary>
        Task AddAsync(Review review);

        /// <summary>
        /// Lưu (theo chuẩn chung Repo)
        /// </summary>
        Task SaveChangesAsync();
    }
}
