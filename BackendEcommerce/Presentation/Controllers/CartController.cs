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
    [Authorize] // BẮT BUỘC Đăng nhập 
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

       

        /// <summary>
        /// (Gạch 4) API "Refresh" (Chậm & Mới)
        /// Lấy và Đồng bộ Giỏ hàng (dùng cho Trang Giỏ hàng đầy đủ)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponseDTO<CartSnapshotDto>>> GetCart()
        {
            var customerId = GetCurrentCustomerId();
            var response = await _cartService.GetAndRefreshCartAsync(customerId);

            // (API [GET] thường chỉ trả về 200)
            return Ok(response);
        }

        /// <summary>
        /// (Gạch 2) API Thêm/Sửa (Nhanh)
        /// Thêm mới, hoặc Cộng dồn, hoặc Cập nhật Số lượng
        /// </summary>
        [HttpPost("items")]
        public async Task<ActionResult<ApiResponseDTO<int>>> AddOrUpdateItem(
            [FromBody] AddCartItemRequestDto dto)
        {
            var customerId = GetCurrentCustomerId();
            var response = await _cartService.AddOrUpdateItemAsync(customerId, dto);

            if (!response.IsSuccess)
            {
                // (Lỗi 400 Tồn kho, 404 Sản phẩm)
                return StatusCode(400, response);
            }

            // Trả về 200 OK với (Count) mới
            return Ok(response);
        }

        /// <summary>
        /// (Gạch 3) API Xóa (Nhanh)
        /// Xóa 1 món hàng (dựa trên VariantId) khỏi giỏ
        /// </summary>
        [HttpDelete("items/{variantId}")]
        public async Task<ActionResult<ApiResponseDTO<int>>> DeleteItem(int variantId)
        {
            var customerId = GetCurrentCustomerId();

            // (Giao việc cho Service, nó luôn trả về 200 OK)
            var response = await _cartService.DeleteItemAsync(customerId, variantId);

            // Trả về 200 OK với (Count) mới
            return Ok(response);
        }
        [HttpDelete] 
        public async Task<ActionResult<ApiResponseDTO<int>>> ClearMyCart()
        {
            var customerId = GetCurrentCustomerId();

            // Gọi hàm Service mới
            var response = await _cartService.ClearCartAsync(customerId);

            // (API này chỉ trả về "Count" mới, luôn là 0)
            return Ok(response);
        }
        // (Hàm trợ giúp lấy CustomerId từ Token)
        private int GetCurrentCustomerId()
        {
            var customerIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // (Chúng ta có thể check TryParse, nhưng [Authorize] đã đảm bảo)
            return int.Parse(customerIdString!);
        }
    }
}
