using BackendEcommerce.Application.Features.Categories.Contracts;
using BackendEcommerce.Application.Features.Categories.DTOs;
using BackendEcommerce.Application.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendEcommerce.Presentation.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Lấy tất cả danh mục (Dùng cho dropdown khi tạo/lọc sản phẩm)
        /// </summary>
        [HttpGet]
        [AllowAnonymous] // Ai cũng xem được Category
        [ProducesResponseType(typeof(ApiResponseDTO<List<RecursiveCategoryDto>>), 200)]
        public async Task<ActionResult<ApiResponseDTO<List<RecursiveCategoryDto>>>> GetAllCategories()
        {
            var response = await _categoryService.GetAllTreeAsync();
            return Ok(response);
        }
    }
}
