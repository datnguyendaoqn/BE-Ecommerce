using BackendEcommerce.Application.Features.Categories.Contracts;
using BackendEcommerce.Infrastructure.Persistence.Data;
using BackendEcommerce.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendEcommerce.Infrastructure.Persistence.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly EcomDbContext _context;

        public CategoryRepository(EcomDbContext context)
        {
            _context = context;
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            // FindAsync là cách nhanh nhất để lấy bằng Khóa chính (Primary Key)
            return await _context.Categories.FindAsync(id);
        }

        public async Task<List<Category>> GetAllAsync()
        {
            // Lấy tất cả và trả về 1 List
            return await _context.Categories.AsNoTracking().ToListAsync();
        }
    }
}
