using BackendEcommerce.Infrastructure.Persistence.Models;

namespace BackendEcommerce.Application.Features.Categories.Contracts
{
    public interface ICategoryRepository
    {
        /// <summary>
        /// Lấy một Category bằng ID (Dùng cho ProductService validation)
        /// </summary>
        Task<Category?> GetByIdAsync(int id);

        /// <summary>
        /// Lấy TẤT CẢ Category (Dùng cho API GET /api/categories)
        /// </summary>
        Task<List<Category>> GetAllAsync();

        // Chúng ta sẽ KHÔNG thêm AddAsync, UpdateAsync, DeleteAsync vội

    }
}
