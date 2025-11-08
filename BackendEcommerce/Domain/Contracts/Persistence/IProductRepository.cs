using BackendEcommerce.Infrastructure.Persistence.Models;

namespace BackendEcommerce.Domain.Contracts.Persistence
{
    public interface IProductRepository
    {
        Task AddAsync(Product product);
        Task SaveChangesAsync();
        // (We will add Get/Update/Delete methods here later)
        Task<IEnumerable<Product>> GetProductsByShopIdAsync(int shopId);
        Task<Product?> GetProductDetailByIdAsync(int productId);
        Task<bool> ExistsAsync(int productId);
        void Update(Product product);
        Task<Product?> GetProductForUpdateAsync(int productId);
    }

}
