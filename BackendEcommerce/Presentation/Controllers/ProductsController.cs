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
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost]
        [Authorize(Roles = "seller")] // Only Sellers can create
        [Consumes("multipart/form-data")] // IMPORTANT: Tell Swagger this is not JSON
        [ProducesResponseType(typeof(ApiResponseDTO<ProductResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponseDTO<ProductResponseDto>), 400)]
        [ProducesResponseType(typeof(ApiResponseDTO<ProductResponseDto>), 403)]
        public async Task<ActionResult<ApiResponseDTO<ProductResponseDto>>> CreateProduct(
            [FromForm] CreateProductRequestDto dto)
        {
            // Get the Seller's ID from their JWT Token
            var sellerIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(sellerIdString))
            {
                return Unauthorized(new ApiResponseDTO<ProductResponseDto> { Message = "Invalid token." });
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
    }
}
