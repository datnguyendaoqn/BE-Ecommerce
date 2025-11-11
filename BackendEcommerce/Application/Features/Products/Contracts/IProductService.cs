using BackendEcommerce.Application.Features.Products.DTOs;
using BackendEcommerce.Application.Shared.DTOs;

namespace BackendEcommerce.Application.Features.Products.Contracts
{
    public interface IProductService
    {
        // We get the DTO and the Seller's ID (from JWT)
        Task<ApiResponseDTO<CreateProductResponseDto>> CreateProductAsync(CreateProductRequestDto dto, int sellerId);
        Task<ApiResponseDTO<List<ProductSummaryResponseDto>>> GetProductsForSellerAsync(int sellerId);
        Task<ApiResponseDTO<ProductDetailResponseDto>> GetProductDetailForSellerAsync(int productId, int sellerId);
        Task<ApiResponseDTO<UpdateProductResponseDto>> UpdateProductAsync
            (int productId, UpdateProductRequestDto dto, int sellerId);
        Task<ApiResponseDTO<UpdateProductVariantResponseDto>> UpdateProductVariantAsync
            (int productId,int variantId,UpdateProductVariantRequestDto dto,int sellerId);
        Task<ApiResponseDTO<ProductVariantDetailDto>> AddVariantAsync(
            int productId, AddVariantRequestDto dto, int sellerId);
        Task<ApiResponseDTO<string>> DeleteVariantAsync(
            int productId, int variantId, int sellerId);
        /// <summary>
        /// [COMMAND] Xóa Cứng (Hard Delete) một Product (cha) và
        /// tất cả Variant (con), Media (cháu).
        /// </summary>
        Task<ApiResponseDTO<string>> DeleteProductAsync(int productId, int sellerId);
        // === KẾT THÚC HÀM MỚI ===
        Task<ApiResponseDTO<PagedListResponseDto<ProductCardDto>>> GetProductListForCustomerAsync
            (ProductListQueryRequestDto query);
    }
}

