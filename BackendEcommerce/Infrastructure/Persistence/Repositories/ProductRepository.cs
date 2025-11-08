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
                // === THÊM .Include() VÀO ĐÂY ===
                .Include(p => p.Category) // Gộp bảng Category vào
                .Where(p => p.ShopId == shopId && p.Status != "deleted")
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<Product?> GetProductDetailByIdAsync(int productId)
        {
            // Đây là query "production-ready"
            // .Include(): Lấy Product (cha)
            // .ThenInclude(): Lấy Variant (con)
            // .Include(): Lấy Shop (để check quyền sở hữu)
            // .Include(): Lấy Category (để lấy tên)
            return await _context.Products
                .Include(p => p.Variants)
                .Include(p => p.Shop)
                .Include(p => p.Category)
                .Where(p => p.Status != "deleted") // Chỉ lấy sản phẩm còn hoạt động
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == productId);
        }
        public async Task<bool> ExistsAsync(int productId)
        {
            // Dùng AnyAsync là cách nhanh nhất để check tồn tại
            return await _context.Products
                .AnyAsync(p => p.Id == productId && p.Status != "deleted");
        }
        public void Update(Product product)
        {
            // Không cần "async", đây là hàm đồng bộ (synchronous)
            // Nó chỉ "báo" cho ChangeTracker của EF Core là "entity này đã bị sửa"
            _context.Products.Update(product);
        }
        public async Task<Product?> GetProductForUpdateAsync(int productId)
        {
            // Chỉ Include() những gì tối thiểu cần cho validation và response
            return await _context.Products
                .Include(p => p.Shop)       // Cần cho check quyền 403
                .Include(p => p.Category)   // Cần để lấy Category.Name
                .FirstOrDefaultAsync(p => p.Id == productId);
        }
    }
}
