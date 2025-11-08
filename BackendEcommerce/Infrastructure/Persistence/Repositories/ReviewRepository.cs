using BackendEcommerce.Domain.Contracts.Persistence;
using BackendEcommerce.Infrastructure.Persistence.Data;
using BackendEcommerce.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendEcommerce.Infrastructure.Persistence.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly EcomDbContext _context;

        public ReviewRepository(EcomDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<Review>> GetReviewsForProductAsync(int productId)
        {
            // Query "production-ready" (chống N+1)
            // Lấy Review
            // ... mà `Variant` (con) của nó thuộc về `ProductId` (cha)
            // ... và gộp (Include) `User` (để lấy tên)
            // ... và gộp (Include) `Variant` (để lấy Size/Màu)
            
            return await _context.Reviews
                .Where(r => r.Variant.ProductId == productId)
                .Include(r => r.User)
                .Include(r => r.Variant)
                .AsNoTracking()
                .OrderByDescending(r => r.CreatedAt) // Mới nhất lên đầu
                .ToListAsync();
        }
    }
}
