using BackendEcommerce.Infrastructure.Persistence.Models;

namespace BackendEcommerce.Application.Features.Products.Contracts
{
    public interface IProductVariantRepository
    {
        /// <summary>
        /// Lấy Variant theo ID, kèm theo Product (cha) và Shop (ông)
        /// (Dùng để check quyền sở hữu)
        /// </summary>
        Task<ProductVariant?> GetVariantWithProductAndShopAsync(int variantId);

        /// <summary>
        // Lấy 1 Variant theo ID (dùng cho Cart/Order)
        /// </summary>
        Task<ProductVariant?> GetByIdAsync(int variantId);

        // (Các hàm này sẽ dùng cho Nhóm 4: Quản lý Variant)
        void Update(ProductVariant variant);
        void Delete(ProductVariant variant);
        Task<bool> IsSkuExistsAsync(string sku);

        // (Giả định SaveChangesAsync được gọi từ UnitOfWork hoặc Service)
        Task SaveChangesAsync();
        Task AddAsync(ProductVariant variant);
        /// <summary>
        /// (Mới) Đếm số variant "active" của một Product
        /// </summary>
        Task<int> CountByProductIdAsync(int productId);

        /// <summary>
        /// (Mới) Lấy danh sách (chỉ) các variant của 1 Product
        /// (Dùng để tính toán lại MinPrice)
        /// </summary>
        Task<List<ProductVariant>> GetVariantsByProductIdAsync(int productId);


    }
}
