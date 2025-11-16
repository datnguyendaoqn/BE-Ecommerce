using BackendEcommerce.Application.Features.Carts.Contracts;
using BackendEcommerce.Application.Features.Carts.DTOs;
using BackendEcommerce.Application.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackendEcommerce.Presentation.Controllers
{
    [Route("api/carts")]
    [ApiController]
    [Authorize(Roles ="customer,seller")] // BẮT BUỘC Đăng nhập 
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        // (Hàm trợ giúp lấy CustomerId từ Token)
        private int GetCurrentCustomerId()
        {
            // (Giả định ID User lưu trong ClaimTypes.NameIdentifier)
            var customerIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // (Nên có check Parse an toàn hơn)
            return int.Parse(customerIdString!);
        }

        /// <summary>
        /// (Tự động "Refresh" Giá/Tồn kho với DB)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponseDTO<GroupedCartResponseDto>>> GetMyCart()
        {
            var customerId = GetCurrentCustomerId();
            var response = await _cartService.GetAndRefreshCartAsync(customerId);

            // (API này trả về Toàn bộ Giỏ hàng DTO)
            return Ok(response);
        }

        /// <summary>
        /// (Dùng cho nút "Thêm vào giỏ")
        /// </summary>
        [HttpPost("items")]
        public async Task<ActionResult<ApiResponseDTO<int>>> AddOrUpdateItem([FromBody] AddCartItemRequestDto dto)
        {
            var customerId = GetCurrentCustomerId();
            var response = await _cartService.AddOrUpdateItemAsync(customerId, dto);

            if (!response.IsSuccess)
            {
                return response.Code switch
                {
                    404 => NotFound(response),
                    400 => BadRequest(response),
                    _ => StatusCode(500, response)
                };
            }

            // (API này chỉ trả về "Count" mới)
            return Ok(response);
        }

        /// <summary>
        /// "GHI ĐÈ" (Set) số lượng
        /// </summary>
        [HttpPut("items")]
        public async Task<ActionResult<ApiResponseDTO<int>>> SetItemQuantity([FromBody] UpdateCartItemRequestDto dto)
        {
            var customerId = GetCurrentCustomerId();
            var response = await _cartService.SetItemQuantityAsync(customerId, dto);

            if (!response.IsSuccess)
            {
                return response.Code switch
                {
                    404 => NotFound(response),
                    400 => BadRequest(response),
                    _ => StatusCode(500, response)
                };
            }

            // (API này chỉ trả về "Count" mới)
            return Ok(response);
        }

        /// <summary>
        /// Xóa 1 món hàng khỏi giỏ
        /// </summary>
        [HttpDelete("items/{variantId}")]
        public async Task<ActionResult<ApiResponseDTO<int>>> DeleteItem(int variantId)
        {
            var customerId = GetCurrentCustomerId();

            // (Xóa theo VariantId, không phải CartItemId, để đơn giản)
            var response = await _cartService.DeleteItemAsync(customerId, variantId);

            // (API này chỉ trả về "Count" mới)
            return Ok(response);
        }

        /// <summary>
        /// Xóa TẤT CẢ món hàng khỏi giỏ
        /// (Dùng cho nút "Clear Cart" trên Trang Giỏ hàng)
        /// </summary>
        [HttpDelete] 
        public async Task<ActionResult<ApiResponseDTO<int>>> ClearMyCart()
        {
            var customerId = GetCurrentCustomerId();

            // Gọi hàm Service mới
            var response = await _cartService.ClearCartAsync(customerId);

            // (API này chỉ trả về "Count" mới, luôn là 0)
            return Ok(response);
        }
    }
}

