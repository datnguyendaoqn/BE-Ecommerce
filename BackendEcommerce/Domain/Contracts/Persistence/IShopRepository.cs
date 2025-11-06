using BackendEcommerce.Infrastructure.Persistence.Models;

namespace BackendEcommerce.Domain.Contracts.Persistence
{
    public interface IShopRepository
    {
        // This is the key method for our security check
        Task<Shop?> GetByOwnerIdAsync(int ownerId);
    }
}
