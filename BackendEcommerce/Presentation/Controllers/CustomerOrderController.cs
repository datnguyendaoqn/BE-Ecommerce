using BackendEcommerce.Application.Features.CustomerOrders.Contracts;
using BackendEcommerce.Application.Features.CustomerOrders.DTOs;
using BackendEcommerce.Application.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Authentication;
using System.Security.Claims;

namespace BackendEcommerce.Presentation.Controllers
{
    [ApiController]
    [Route("api/customer/orders")]
    [Authorize(Roles = "customer,seller")] // CHỈ Customer mới được gọi
    public class CustomerOrderController : ControllerBase
    {
        private readonly ICustomerOrderService _customerOrderService;

        // 1. Inject Interface CỦA CUSTOMER
        public CustomerOrderController(ICustomerOrderService customerOrderService)
        {
            _customerOrderService = customerOrderService;
        }

        /// <summary>
        /// Lấy lịch sử mua hàng
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyOrders([FromQuery] CustomerOrderFilterDto filter)
        {
            try
            {
                var userId = GetCurrentUserId();
                var pagedResult = await _customerOrderService.GetMyOrdersAsync(userId, filter);

                var response = new ApiResponseDTO<PagedListResponseDto<CustomerOrderResponseDto>>
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Lấy lịch sử mua hàng thành công",
                    Data = pagedResult
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponseDTO<object> { IsSuccess = false, Code = 400, Message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy CHI TIẾT một đơn hàng
        /// </summary>
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById(int orderId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var orderDetailDto = await _customerOrderService.GetMyOrderDetailAsync(userId, orderId);

                var response = new ApiResponseDTO<CustomerOrderDetailResponseDto>
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Lấy chi tiết đơn hàng thành công",
                    Data = orderDetailDto
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Bắt các lỗi (vd: Lỗi không tìm thấy, lỗi không có quyền)
                return BadRequest(new ApiResponseDTO<object> { IsSuccess = false, Code = 400, Message = ex.Message });
            }
        }


        /// <summary>
        /// Customer HỦY đơn hàng (chỉ khi 'Pending')
        /// </summary>
        [HttpPatch("{orderId}/cancel")] // Dùng PATCH
        public async Task<IActionResult> CancelOrder(int orderId, [FromBody] CustomerCancelOrderRequestDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                // (Truyền DTO vào Service)
                var updatedOrderDto = await _customerOrderService.CancelMyPendingOrderAsync(userId, orderId, dto);

                var response = new ApiResponseDTO<CustomerOrderResponseDto>
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Hủy đơn hàng thành công",
                    Data = updatedOrderDto
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Bắt các lỗi (vd: Lỗi không tìm thấy, lỗi State Machine...)
                return BadRequest(new ApiResponseDTO<object> { IsSuccess = false, Code = 400, Message = ex.Message });
            }
        }

        /// <summary>
        /// Customer XÁC NHẬN đã nhận hàng (chỉ khi 'Shipped')
        /// </summary>
        [HttpPatch("{orderId}/confirm-delivery")] // Dùng PATCH
        public async Task<IActionResult> ConfirmDelivery(int orderId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var updatedOrderDto = await _customerOrderService.ConfirmMyDeliveryAsync(userId, orderId);

                var response = new ApiResponseDTO<CustomerOrderResponseDto>
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Xác nhận đã nhận hàng thành công",
                    Data = updatedOrderDto
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponseDTO<object> { IsSuccess = false, Code = 400, Message = ex.Message });
            }
        }

        // --- PRIVATE HELPER ---
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                throw new AuthenticationException("Không thể xác định người dùng từ Token.");
            }
            return userId;
        }
    }
}
