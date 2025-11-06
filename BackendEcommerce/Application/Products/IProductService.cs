using BackendEcommerce.Application.Products.DTOs;
using BackendEcommerce.Application.Shared.DTOs;

namespace BackendEcommerce.Application.Products
{
    public interface IProductService
    {
        // We get the DTO and the Seller's ID (from JWT)
        Task<ApiResponseDTO<ProductResponseDto>> CreateProductAsync(CreateProductRequestDto dto, int sellerId);
    }
}
