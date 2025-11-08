using BackendEcommerce.Application.Medias;
using BackendEcommerce.Application.Medias.DTOs;
using BackendEcommerce.Application.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackendEcommerce.Presentation.Controllers
{
    [Route("api/media")] // Đường dẫn gốc cho Media
    [ApiController]
    [Authorize(Roles = "seller")] // Chỉ Seller mới được thao tác
    public class MediaController : ControllerBase
    {
        private readonly IMediaService _mediaService;

        public MediaController(IMediaService mediaService)
        {
            _mediaService = mediaService;
        }

        /// <summary>
        /// [COMMAND] Đặt một ảnh làm ảnh chính (Primary) cho Product.
        /// </summary>
        [HttpPut("product/{productId}/set-primary/{mediaId}")]
        public async Task<ActionResult<ApiResponseDTO<SetPrimaryMediaResponseDto>>> SetProductPrimaryMedia(
            int productId, int mediaId)
        {
            // 1. Lấy SellerId từ Token (theo đúng pattern của bạn)
            var sellerIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(sellerIdString, out var sellerId))
            {
                return Unauthorized(new ApiResponseDTO<SetPrimaryMediaResponseDto>
                {
                    IsSuccess = false,
                    Code = 401,
                    Message = "User không hợp lệ."
                });
            }

            // 2. Giao việc cho "Quản lý" (Service)
            var response = await _mediaService.SetProductPrimaryMediaAsync(productId, mediaId, sellerId);

            // 3. Xử lý Phản hồi (theo đúng pattern của bạn)
            if (!response.IsSuccess)
            {
                return response.Code switch
                {
                    400 => BadRequest(response),          // Lỗi 400 (Đã là primary, Media không thuộc Product)
                    403 => StatusCode(403, response), // Lỗi 403 (Không sở hữu sản phẩm)
                    404 => NotFound(response),          // Lỗi 404 (Không tìm thấy Product hoặc Media)
                    _ => BadRequest(response)
                };
            }

            // 4. Trả về 200 OK (với DTO "nhẹ")
            return Ok(response);
        }
    }
    }
