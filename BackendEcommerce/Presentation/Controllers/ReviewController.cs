using BackendEcommerce.Application.Reviews;
using BackendEcommerce.Application.Reviews.DTOs;
using BackendEcommerce.Application.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendEcommerce.Presentation.Controllers
{
    [ApiController]
    [Route("api/product/{productId}/reviews")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        /// <summary>
        /// (Public) Lấy danh sách review cho 1 sản phẩm
        /// </summary>
        /// <param name="productId">ID của sản phẩm (Lấy từ URL route)</param>
        [HttpGet] // <-- Chỉ cần [HttpGet] vì route đã ở trên
        [AllowAnonymous] // Ai cũng xem được
        [ProducesResponseType(typeof(ApiResponseDTO<List<ReviewResponseDto>>), 200)]
        public async Task<ActionResult<ApiResponseDTO<List<ReviewResponseDto>>>> GetProductReviews(int productId)
        {
            var response = await _reviewService.GetProductReviewsAsync(productId);
            return Ok(response);
        }

        // (Sau này các API [HttpPost] (Tạo Review) cũng sẽ nằm ở đây)
    }
}
