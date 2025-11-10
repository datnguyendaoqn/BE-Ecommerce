using BackendEcommerce.Application.Features.Categories.Contracts;
using BackendEcommerce.Application.Features.Categories.DTOs;
using BackendEcommerce.Application.Shared.DTOs;
using BackendEcommerce.Infrastructure.Persistence.Models;

namespace BackendEcommerce.Application.Features.Categories
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepo;
        // (Chúng ta có thể inject AutoMapper ở đây, nhưng giờ làm thủ công)

        public CategoryService(ICategoryRepository categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        /// <summary>
        /// Lấy TẤT CẢ category (Đã được xây dựng thành "Cây" Đệ quy)
        /// </summary>
        public async Task<ApiResponseDTO<List<RecursiveCategoryDto>>> GetAllTreeAsync()
        {
            // 1. Lấy danh sách "phẳng" (flat list) từ "Tay chân" (Repo)
            var allCategories = await _categoryRepo.GetAllAsync();

            // 2. Gọi hàm "Não bộ" (private helper) để xây "Cây"
            // Bắt đầu từ "gốc" (parentId = null)
            var categoryTree = BuildCategoryTree(allCategories, null);

            return new ApiResponseDTO<List<RecursiveCategoryDto>>
            {
                IsSuccess = true,
                Data = categoryTree
            };
        }

        /// <summary>
        /// Đây là logic "Đệ quy" (Recursive) để biến đổi
        /// một List (phẳng) thành một Cấu trúc Cây (Tree)
        /// </summary>
        private List<RecursiveCategoryDto> BuildCategoryTree(List<Category> allCategories, int? parentId)
        {
            // 1. Tìm tất cả "con" trực tiếp của "cha" (parentId) này
            var children = allCategories
                .Where(c => c.ParentId == parentId)
                .ToList();

            if (!children.Any())
            {
                return new List<RecursiveCategoryDto>(); // Điều kiện dừng (Không có con)
            }

            // 2. Biến đổi (Map) chúng sang DTO
            var dtoChildren = children.Select(c => new RecursiveCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                ParentId = c.ParentId,

                // 3. (Đệ quy) Lấy "cháu"
                // Gọi lại chính hàm này, nhưng bâyA:
                // "Cha" (parentId) mới là "Con" (c.Id) hiện tại
                Children = BuildCategoryTree(allCategories, c.Id)

            }).ToList();

            return dtoChildren;
        }
    }
}

