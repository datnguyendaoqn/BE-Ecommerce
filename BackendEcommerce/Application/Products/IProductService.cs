using BackendEcommerce.Application.Products.DTOs;
using BackendEcommerce.Application.Shared.DTOs;

namespace BackendEcommerce.Application.Products
{
    public interface IProductService
    {
        // We get the DTO and the Seller's ID (from JWT)
        Task<ApiResponseDTO<CreateProductResponseDto>> CreateProductAsync(CreateProductRequestDto dto, int sellerId);
        Task<ApiResponseDTO<List<ProductSummaryResponseDto>>> GetProductsForSellerAsync(int sellerId);
        Task<ApiResponseDTO<ProductDetailResponseDto>> GetProductDetailForSellerAsync(int productId, int sellerId);
        Task<ApiResponseDTO<ProductDetailResponseDto>> UpdateProductAsync(int productId, UpdateProductRequestDto dto, int sellerId);
    }
}
