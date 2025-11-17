using BackendEcommerce.Application.Features.Reviews.Contracts;
using BackendEcommerce.Application.Features.Reviews.DTOs;
using BackendEcommerce.Application.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackendEcommerce.Presentation.Controllers
{
    [ApiController]
    [Route("api")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        /// <summary>
        /// Lấy danh sách review cho 1 sản phẩm
        /// </summary>
        /// <param name="productId">ID của sản phẩm (Lấy từ URL route)</param>
        [HttpGet("products/{productId}/reviews")] // <-- Chỉ cần [HttpGet] vì route đã ở trên
        [AllowAnonymous] // Ai cũng xem được
        [ProducesResponseType(typeof(ApiResponseDTO<List<ReviewProductResponseDto>>), 200)]
        public async Task<ActionResult<ApiResponseDTO<List<ReviewProductResponseDto>>>> GetProductReviews(int productId)
        {
            var response = await _reviewService.GetProductReviewsAsync(productId);
            return Ok(response);
        }
        /// <summary>
        // (MỚI) API để Customer tạo một Review mới
        /// </summary>
        /// <param name="dto">Dữ liệu review (OrderItemId, Rating, Comment)</param>
        /// <returns></returns>
        [HttpPost("reviews")]
        [Authorize] // BẮT BUỘC: Chỉ user đã đăng nhập mới được review
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequestDto dto)
        {
            // 1. Lấy UserId từ Token (Context)
            // Đây là cách chuẩn để lấy UserId, an toàn 100%
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                // Nếu Token không hợp lệ hoặc không chứa UserId
                return StatusCode(401, new ApiResponseDTO<string> { IsSuccess = false, Code = 401, Message = "Token không hợp lệ hoặc không tìm thấy thông tin người dùng." });
            }

            // 2. Giao hết việc cho Service
            var response = await _reviewService.CreateReviewAsync(dto, userId);

            // 3. Trả về Response từ Service (đã chuẩn hóa)
            if (!response.IsSuccess)
            {
                // Các mã lỗi 400, 403, 404, 500...
                return StatusCode(400, response);
            }

            return CreatedAtAction(nameof(GetProductReviews), new { productId = "unknown" }, response);
            // Lưu ý: Chúng ta không biết productId ở đây, nên FE chỉ cần đọc Data
        }

    }
}
