using BackendEcommerce.Application.Features.Products.Contracts;
using BackendEcommerce.Application.Features.Products.DTOs;
using BackendEcommerce.Application.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BackendEcommerce.Presentation.Controllers
{
    [ApiController]
    [Route("api/products")]
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
        [HttpPut("{productId}")]
        [Authorize(Roles = "seller")]
        public async Task<ActionResult<ApiResponseDTO<UpdateProductResponseDto>>> UpdateProduct(int productId,[FromBody] UpdateProductRequestDto dto)
        {
            // 1. Lấy SellerId từ Token
            var sellerIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(sellerIdString, out var sellerId))
            {
                return Unauthorized(new ApiResponseDTO<UpdateProductResponseDto>
                {
                    IsSuccess = false,
                    Code = 401,
                    Message = "User không hợp lệ."
                });
            }

            // 2. Giao việc cho "Quản lý" (Service)
            var response = await _productService.UpdateProductAsync(productId, dto, sellerId);

            // 3. Xử lý Phản hồi (theo đúng pattern của bạn)
            if (!response.IsSuccess)
            {
                // Service đã check 400 (Category), 403 (Quyền), 404 (Not Found)
                return response.Code switch
                {
                    400 => BadRequest(response),          // Lỗi 400 (ví dụ: CategoryId mới không tồn tại)
                    403 => StatusCode(403, response), // Lỗi 403 (Không sở hữu sản phẩm)
                    404 => NotFound(response),          // Lỗi 404 (Không tìm thấy sản phẩm)
                    _ => BadRequest(response)           // Các lỗi khác
                };
            }

            // 4. Trả về 200 OK (với DTO "nhẹ" - UpdateProductResponseDto)
            return Ok(response);
        }
        //
        //
        // === CHỨC NĂNG 4B: THÊM VARIANT MỚI ===
        [HttpPost("{productId}/variants")]
        [Authorize(Roles = "seller")]
        [Consumes("multipart/form-data")] // QUAN TRỌNG: Vì có IFormFile
        public async Task<ActionResult<ApiResponseDTO<ProductVariantDetailDto>>> AddVariant(
            int productId,
            [FromForm] AddVariantRequestDto dto)
        {
            // 1. Lấy SellerId từ Token
            var sellerIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(sellerIdString, out var sellerId))
            {
                return Unauthorized(new ApiResponseDTO<ProductVariantDetailDto> { Message = "Invalid token." });
            }

            // 2. Giao việc cho Service (đã bao gồm Transaction)
            var response = await _productService.AddVariantAsync(productId, dto, sellerId);

            // 3. Dịch kết quả (Theo khuôn mẫu của bạn)
            if (!response.IsSuccess)
            {
                return response.Code switch
                {
                    400 => BadRequest(response),          // Lỗi (SKU, Size/Color)
                    403 => StatusCode(403, response), // Cấm (Không sở hữu)
                    404 => NotFound(response),            // Không tìm thấy
                    _ => StatusCode(500, response)    // Lỗi (Transaction, Upload)
                };
            }

            // Trả về 200 OK (hoặc 201 Created) với DTO "nặng"
            return Ok(response);
        }
        //
        //
        [HttpGet]
        public async Task<IActionResult> GetProductList([FromQuery] ProductListQueryRequestDto query)
        {
            // Giao hết việc cho Service
            var response = await _productService.GetProductListForCustomerAsync(query);

            if (!response.IsSuccess)
            {
                // Đây là API public, lỗi thường là 500
                return StatusCode(500, response);
            }

            // Trả về 200 OK với PagedListResponseDto<ProductCardDto>
            return Ok(response);
        }

        // (Sau này chúng ta sẽ thêm [HttpGet("{id}")]
        // để gọi hàm GetProductDetailForBuyerAsync tại đây)
    }
}
