using BackendEcommerce.Infrastructure.Persistence.Models;

namespace BackendEcommerce.Domain.Contracts.Persistence
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
        void Add(ProductVariant variant);
        void Update(ProductVariant variant);
        void Delete(ProductVariant variant);

        // (SaveChangesAsync sẽ được gọi chung qua IUnitOfWork 
        // hoặc gọi trực tiếp từ Service nếu Repo có)
        // Task SaveChangesAsync(); 
    }
}
