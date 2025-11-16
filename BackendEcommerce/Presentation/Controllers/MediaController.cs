using BackendEcommerce.Application.Features.Medias.Contracts;
using BackendEcommerce.Application.Features.Medias.DTOs;
using BackendEcommerce.Application.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackendEcommerce.Presentation.Controllers
{
    [Route("api/medias")] // Đường dẫn gốc cho Media
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
        /// Swap một ảnh làm ảnh chính (Primary) cho Product.
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
        //
        //
        [HttpPost("product/{productId}/add-gallery")]
        [Consumes("multipart/form-data")] // <-- Bắt buộc cho Upload
        public async Task<ActionResult<ApiResponseDTO<AddGalleryImagesResponseDto>>> AddGalleryImages(
        int productId, [FromForm] List<IFormFile> images) // <-- [FromForm]
        {
            // 1. Lấy SellerId từ Token 
            var sellerIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(sellerIdString, out var sellerId))
            {
                return Unauthorized(new ApiResponseDTO<AddGalleryImagesResponseDto>
                {
                    IsSuccess = false,
                    Code = 401,
                    Message = "User không hợp lệ."
                });
            }

            // 2. Giao việc cho "Quản lý" (Service)
            var response = await _mediaService.AddGalleryImagesAsync(productId, images, sellerId);

            // 3. Xử lý Phản hồi 
            if (!response.IsSuccess)
            {
                return response.Code switch
                {
                    400 => BadRequest(response),          // Lỗi 400 (Không có file)
                    403 => StatusCode(403, response), // Lỗi 403 (Không sở hữu)
                    404 => NotFound(response),          // Lỗi 404 (Không tìm thấy Product)
                    _ => StatusCode(500, response)    // Lỗi 500 (Upload/DB lỗi)
                };
            }

            // 4. Trả về 200 OK (với DTO "pragmatic" chứa list ảnh mới)
            return Ok(response);
        }
        //
        //
        [HttpDelete("{mediaId}")]
        public async Task<ActionResult<ApiResponseDTO<string>>> DeleteMedia(int mediaId)
        {
            // 1. Lấy SellerId từ Token
            var sellerIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(sellerIdString, out var sellerId))
            {
                return Unauthorized(new ApiResponseDTO<string>
                {
                    IsSuccess = false,
                    Code = 401,
                    Message = "User không hợp lệ."
                });
            }

            // 2. Giao việc cho "Quản lý" (Service)
            var response = await _mediaService.DeleteMediaAsync(mediaId, sellerId);

            // 3. Xử lý Phản hồi
            if (!response.IsSuccess)
            {
                return response.Code switch
                {
                    400 => BadRequest(response),          // Lỗi 400 (Là ảnh Primary)
                    403 => StatusCode(403, response), // Lỗi 403 (Không sở hữu)
                    404 => NotFound(response),          // Lỗi 404 (Không tìm thấy Media)
                    _ => StatusCode(500, response)    // Lỗi 500 (Cloudinary/DB lỗi)
                };
            }

            // 4. Trả về 200 OK (chỉ là message)
            return Ok(response);
        }
    }
    }
