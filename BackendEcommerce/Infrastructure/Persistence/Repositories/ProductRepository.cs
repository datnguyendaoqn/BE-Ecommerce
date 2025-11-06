using BackendEcommerce.Domain.Contracts.Persistence;
using BackendEcommerce.Infrastructure.Persistence.Data;
using BackendEcommerce.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendEcommerce.Infrastructure.Persistence.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly EcomDbContext _context;

        public ProductRepository(EcomDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<Product>> GetProductsByShopIdAsync(int shopId)
        {
            // Lấy tất cả sản phẩm (trừ các sản phẩm đã bị "xóa mềm")
            // Chỉ select các cột cần thiết cho "ProductSummaryDto" (Tối ưu)
            return await _context.Products
                .Where(p => p.ShopId == shopId && p.Status != "deleted")
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new Product // Chỉ lấy các cột cần thiết
                {
                    Id = p.Id,
                    Name = p.Name,
                    MinPrice = p.MinPrice,
                    VariantCount = p.VariantCount,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();
        }
    }
}
