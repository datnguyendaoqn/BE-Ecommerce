using BackendEcommerce.Application.Features.Orders.Contracts;
using BackendEcommerce.Application.Features.Orders.DTOs;
using BackendEcommerce.Application.Shared.DTOs;
using BackendEcommerce.Infrastructure.Persistence.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackendEcommerce.Presentation.Controllers
{
    [Route("api/orders")]
    [ApiController]
    [Authorize] // (BẮT BUỘC Đăng nhập)
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // (Hàm trợ giúp lấy CustomerId từ Token)
        private int GetCurrentCustomerId()
        {
            var customerIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // (Nên có check Parse an toàn hơn)
            return int.Parse(customerIdString!);
        }

        /// <summary>
        /// [API (Giao diện Lập trình Ứng dụng) CỐT LÕI (CORE)] Đặt hàng (Place Order) (Đa Cửa hàng (Multi-Shop))
        /// (Dùng Giỏ hàng (Cart) (Redis) và DTO (Input) Địa chỉ (Address))
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponseDTO<CreateOrderResponseDto>>> CreateOrder(
            [FromBody] CreateOrderRequestDto dto)
        {
            var customerId = GetCurrentCustomerId();
            var response = await _orderService.CreateOrderAsync(customerId, dto);

            if (!response.IsSuccess)
            {
                // (Các lỗi đã được xử lý bởi Service (Dịch vụ): 400 (Hết hàng/Giỏ hàng (Cart) rỗng/Giá (Price) thay đổi), 404 (Địa chỉ (Address) sai), 500 (Lỗi DB (Cơ sở dữ liệu)))
                return response.Code switch
                {
                    404 => NotFound(response),
                    400 => BadRequest(response),
                    _ => StatusCode(500, response)
                };
            }

            // (Trả về (Return) 201 Created (hoặc 200 OK) với List (Danh sách) OrderId (ID Đơn hàng) MỚI)
            return Ok(response);
        }
    }
}
