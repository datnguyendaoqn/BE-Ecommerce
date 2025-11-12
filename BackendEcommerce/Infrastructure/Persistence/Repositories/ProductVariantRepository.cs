using BackendEcommerce.Application.Features.Products.Contracts;
using BackendEcommerce.Infrastructure.Persistence.Data;
using BackendEcommerce.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendEcommerce.Infrastructure.Persistence.Repositories
{
    public class ProductVariantRepository : IProductVariantRepository
    {
        private readonly EcomDbContext _context;

        public ProductVariantRepository(EcomDbContext context)
        {
            _context = context;
        }

        public async Task<ProductVariant?> GetVariantWithProductAndShopAsync(int variantId)
        {
            // Lấy Con -> Cha (Product) -> Ông (Shop)
            return await _context.ProductVariants
                .Include(v => v.Product)
                    .ThenInclude(p => p.Shop) // Include lồng nhau
                .FirstOrDefaultAsync(v => v.Id == variantId);
        }

        public async Task<ProductVariant?> GetByIdAsync(int variantId)
        {
            return await _context.ProductVariants
                .Include(v => v.Product)
                 .ThenInclude(p => p.Shop)
                .FirstOrDefaultAsync(v => v.Id == variantId);
        }
        public async Task<List<ProductVariant>> GetVariantsByIdsAsync(List<int> variantIds)
        {
            return await _context.ProductVariants
                .Where(v => variantIds.Contains(v.Id))
                .Include(v => v.Product) // (Cũng cần Include Product)
                 .ThenInclude(p => p.Shop)
                .ToListAsync();
        }
        public async Task AddAsync(ProductVariant variant)
        {
            await _context.ProductVariants.AddAsync(variant);
        }

        public void Update(ProductVariant variant)
        {
            _context.Entry(variant).State = EntityState.Modified;
        }

        public void Delete(ProductVariant variant)
        {
            _context.ProductVariants.Remove(variant);
        }
        public async Task<bool> IsSkuExistsAsync(string sku)
        {
            // Check SKU không phân biệt chữ hoa/thường
            return await _context.ProductVariants
                .AnyAsync(v => v.SKU.ToLower() == sku.ToLower());
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<int> CountByProductIdAsync(int productId)
        {
            // (Sau này nếu dùng Xóa Mềm, phải thêm .Where(v => v.Status == "active"))
            return await _context.ProductVariants
                .CountAsync(v => v.ProductId == productId);
        }

        public async Task<List<ProductVariant>> GetVariantsByProductIdAsync(int productId)
        {
            // (Sau này nếu dùng Xóa Mềm, phải thêm .Where(v => v.Status == "active"))
            return await _context.ProductVariants
                .Where(v => v.ProductId == productId)
                .AsNoTracking() // Chỉ đọc để tính MinPrice
                .ToListAsync();
        }
      
    }
}
