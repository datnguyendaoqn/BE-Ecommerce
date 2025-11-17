using BackendEcommerce.Application.Features.Reviews.Contracts;
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
        public async Task<bool> HasAlreadyReviewedAsync(int orderItemId)
        {
            return await _context.Reviews
                .AnyAsync(r => r.OrderItemId == orderItemId);
        }

        // === (TRIỂN KHAI HÀM MỚI 2/3) ===
        public async Task AddAsync(Review review)
        {
            await _context.Reviews.AddAsync(review);
        }

        // === (TRIỂN KHAI HÀM MỚI 3/3) ===
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
