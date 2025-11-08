using BackendEcommerce.Application.Products;
using BackendEcommerce.Application.Products.DTOs;
using BackendEcommerce.Application.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BackendEcommerce.Presentation.Controllers
{
    [ApiController]
    [Route("api/product")]
    [Authorize] // Only authenticated users can access
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost]
        [Authorize(Roles = "seller")] // Only Sellers can create
        [Consumes("multipart/form-data")] // IMPORTANT: Tell Swagger this is not JSON
        [ProducesResponseType(typeof(ApiResponseDTO<CreateProductResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponseDTO<CreateProductResponseDto>), 400)]
        [ProducesResponseType(typeof(ApiResponseDTO<CreateProductResponseDto>), 403)]
        public async Task<ActionResult<ApiResponseDTO<CreateProductResponseDto>>> CreateProduct(
            [FromForm] CreateProductRequestDto dto)
        {
            // Get the Seller's ID from their JWT Token
            var sellerIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(sellerIdString))
            {
                return Unauthorized(new ApiResponseDTO<CreateProductResponseDto> { Message = "Invalid token." });
            }

            var sellerId = int.Parse(sellerIdString); // Assumes your AccountId is an int

            var response = await _productService.CreateProductAsync(dto, sellerId);

            if (!response.IsSuccess)
            {
                return response.Code 
                switch
                {
                    403 => StatusCode(403,response),
                    404 => StatusCode(404, response),
                    500 => StatusCode(500, response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }
        //
        [HttpGet("my-shop")]
        [Authorize(Roles = "seller")]
        [ProducesResponseType(typeof(ApiResponseDTO<List<ProductSummaryResponseDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponseDTO<List<ProductSummaryResponseDto>>), 403)]
        [ProducesResponseType(typeof(ApiResponseDTO<List<ProductSummaryResponseDto>>), 401)]
        public async Task<ActionResult<ApiResponseDTO<List<ProductSummaryResponseDto>>>> GetMyProducts()
        {
            // 1. Lấy SellerId từ Token (đã xác thực)
            var sellerIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(sellerIdString, out var sellerId))
            {
                return Unauthorized(new ApiResponseDTO<List<ProductSummaryResponseDto>> { IsSuccess = false, Code = 401, Message = "User không hợp lệ." });
            }

            // 2. Giao việc cho "Quản lý"
            var response = await _productService.GetProductsForSellerAsync(sellerId);

            // 3. Trả kết quả
            if (!response.IsSuccess)
            {
                // Chỉ có thể là lỗi 403 (Không phải seller)
                return StatusCode(403, response);
            }

            return Ok(response);
        }
        [HttpGet("{productId}")] // Ví dụ: GET /api/products/123
        [Authorize(Roles = "seller")]
        [ProducesResponseType(typeof(ApiResponseDTO<ProductDetailResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponseDTO<ProductDetailResponseDto>), 403)]
        [ProducesResponseType(typeof(ApiResponseDTO<ProductDetailResponseDto>), 404)]
        public async Task<ActionResult<ApiResponseDTO<ProductDetailResponseDto>>> GetProductDetail(int productId)
        {
            // 1. Lấy SellerId từ Token
            var sellerIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(sellerIdString, out var sellerId))
            {
                return Unauthorized(new ApiResponseDTO<string> { Message = "Invalid token." });
            }

            // 2. Giao hết logic cho "Quản lý" (Service)
            var response = await _productService.GetProductDetailForSellerAsync(productId, sellerId);

            // 3. Dịch kết quả
            if (!response.IsSuccess)
            {
                return response.Code switch
                {
                    403 => StatusCode(403, response), // Cấm (Không sở hữu)
                    404 => NotFound(response),      // Không tìm thấy
                    _ => BadRequest(response)       // Lỗi khác
                };
            }

            return Ok(response);
        }
    }
}
