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
        /// <summary>
        /// (MỚI) Lấy Entity Product (Service cần thông tin gốc)
        /// </summary>
        Task<Product?> GetProductEntityByIdAsync(int productId);

        /// <summary>
        /// (SỬA ĐỔI) Query tối ưu để lấy danh sách ProductCardDto
        /// (Lọc theo Shop VÀ loại trừ sản phẩm hiện tại)
        /// </summary>
        Task<List<ProductCardDto>> GetRelatedProductsAsCardsAsync(
            int shopId, // Không cần nullable nữa
                        // int? categoryId, // ĐÃ XÓA
            int currentProductId,
            int limit);
        /// <summary>
        /// Lấy ProductId (cha) từ VariantId (con)
        /// </summary>
        Task<int?> GetProductIdFromVariantIdAsync(int? variantId);

        /// <summary>
        /// Cập nhật 2 cột thống kê (AverageRating, ReviewCount) của Product
        /// </summary>
        Task UpdateProductRatingStatsAsync(int productId, int newReviewCount, decimal newAverageRating);
        Task IncreaseSelledCountAsync(int variantId, int quantity);
        Task<List<ProductCardDto>> GetBestSellingProductsAsCardsAsync(int limit);
    }

}
