using BackendEcommerce.Application.Features.Categories.DTOs;
using BackendEcommerce.Application.Shared.DTOs;

namespace BackendEcommerce.Application.Features.Categories.Contracts
{
    public interface ICategoryService
    {
        Task<ApiResponseDTO<List<RecursiveCategoryDto>>> GetAllTreeAsync();
    }
}
