using BackendEcommerce.Domain.Contracts.Persistence;
using BackendEcommerce.Infrastructure.Persistence.Data;
using BackendEcommerce.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendEcommerce.Infrastructure.Persistence.Repositories
{
    public class ShopRepository : IShopRepository
    {
        private readonly EcomDbContext _context;

        public ShopRepository(EcomDbContext context)
        {
            _context = context;
        }

        public async Task<Shop?> GetByOwnerIdAsync(int ownerId)
        {
            // Find the shop that belongs to this user (Seller)
            return await _context.Shops.FirstOrDefaultAsync(s => s.OwnerId == ownerId);
        }
    }
}
