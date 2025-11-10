using BackendEcommerce.Domain.Contracts.Persistence;
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
                .FirstOrDefaultAsync(v => v.Id == variantId);
        }

        public void Add(ProductVariant variant)
        {
            _context.ProductVariants.Add(variant);
        }

        public void Update(ProductVariant variant)
        {
            _context.Entry(variant).State = EntityState.Modified;
        }

        public void Delete(ProductVariant variant)
        {
            _context.ProductVariants.Remove(variant);
        }
    }
}
