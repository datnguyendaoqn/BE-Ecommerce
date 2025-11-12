using BackendEcommerce.Application.Features.Products.DTOs;
using BackendEcommerce.Application.Shared.DTOs;
using BackendEcommerce.Infrastructure.Persistence.Models;

namespace BackendEcommerce.Application.Features.Products.Contracts
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
        void Delete(Product product);
        Task<PagedListResponseDto<ProductCardDto>> GetPaginatedProductCardsAsync(ProductListQueryRequestDto query);
    }

}
