using BackendEcommerce.Infrastructure.Persistence.Models;

namespace BackendEcommerce.Domain.Contracts.Persistence
{
    public interface IProductRepository
    {
        Task AddAsync(Product product);
        Task SaveChangesAsync();
        // (We will add Get/Update/Delete methods here later)
    }
}
