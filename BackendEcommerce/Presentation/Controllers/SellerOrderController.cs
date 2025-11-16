using BackendEcommerce.Application.Features.Orders.DTOs;
using BackendEcommerce.Application.Features.SellerOrders.Contracts;
using BackendEcommerce.Application.Features.SellerOrders.DTOs;
using BackendEcommerce.Application.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Authentication;
using System.Security.Claims;

namespace BackendEcommerce.Presentation.Controllers
{
    [ApiController]
    [Route("api/seller/orders")]
    [Authorize(Roles = "seller")] // CHỈ Seller mới được gọi các API trong này
    public class SellerOrderController : ControllerBase
    {
        private readonly ISellerOrderService _sellerOrderService;

        // 1. Inject Interface CỦA SELLER
        public SellerOrderController(ISellerOrderService sellerOrderService)
        {
            _sellerOrderService = sellerOrderService;
        }

        /// <summary>
        /// Lấy lịch sử đơn hàng của Shop (đã phân trang và lọc)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyOrders([FromQuery] OrderFilterDto filter)
        {
            try
            {
                // Lấy UserId từ Token (đã được xác thực)
                var userId = GetCurrentUserId();

                // Gọi Service
                var pagedResult = await _sellerOrderService.GetShopOrdersAsync(userId, filter);

                // Gói kết quả thành công
                var response = new ApiResponseDTO<PagedListResponseDto<OrderSellerResponseDto>>
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Lấy danh sách đơn hàng thành công",
                    Data = pagedResult
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Trả về lỗi chuẩn
                return BadRequest(new ApiResponseDTO<object>
                {
                    IsSuccess = false,
                    Code = 400,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Cập nhật trạng thái một đơn hàng
        /// </summary>
        /// <param name="orderId">ID của đơn hàng (lấy từ Route)</param>
        /// <param name="requestDto">Body chứa NewStatus</param>
        [HttpPatch("{orderId}/status")] // Dùng PATCH vì chỉ cập nhật 1 phần
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] SellerUpdateOrderStatusRequestDto requestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDTO<object> { IsSuccess = false, Code = 400, Message = "Dữ liệu không hợp lệ" });
            }

            try
            {
                var userId = GetCurrentUserId();

                // Gọi Service (nơi chứa State Machine)
                var updatedOrderDto = await _sellerOrderService.UpdateOrderStatusAsync(
                    userId,
                    orderId,
                    requestDto.NewStatus
                );

                // Gói kết quả thành công
                var response = new ApiResponseDTO<OrderSellerResponseDto>
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Cập nhật trạng thái đơn hàng thành công",
                    Data = updatedOrderDto // Trả về đơn hàng đã được cập nhật
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Bắt các lỗi nghiệp vụ (vd: Lỗi không tìm thấy, lỗi State Machine...)
                return BadRequest(new ApiResponseDTO<object>
                {
                    IsSuccess = false,
                    Code = 400,
                    Message = ex.Message
                });
            }
        }


        // --- PRIVATE HELPER ---

        /// <summary>
        /// Hàm helper để lấy UserId từ Claim của Token
        /// </summary>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                // Lỗi này không nên xảy ra nếu [Authorize] hoạt động đúng
                throw new AuthenticationException("Không thể xác định người dùng từ Token.");
            }
            return userId;
        }
    }
}
