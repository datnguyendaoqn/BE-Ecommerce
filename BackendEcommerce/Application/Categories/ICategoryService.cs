using BackendEcommerce.Application.Categories.DTOs;
using BackendEcommerce.Application.Shared.DTOs;

namespace BackendEcommerce.Application.Categories
{
    public interface ICategoryService
    {
        Task<ApiResponseDTO<List<RecursiveCategoryDto>>> GetAllTreeAsync();
    }
}
