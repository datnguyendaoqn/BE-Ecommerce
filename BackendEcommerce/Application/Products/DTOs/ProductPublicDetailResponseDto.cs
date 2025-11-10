using BackendEcommerce.Application.Medias.DTOs;
using BackendEcommerce.Application.Shared.DTOs;
namespace BackendEcommerce.Application.Products.DTOs
{
    public class ProductPublicDetailResponseDto
    {
        // Thông tin cơ bản (Giống Seller)
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Brand { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        // Ảnh (Giống Seller)
        public List<ProductMediaDto> ProductImages { get; set; } = new List<ProductMediaDto>();

        // Biến thể (DTO Mới - Đã ẩn SKU/Quantity)
        public List<ProductVariantPublicDetailDto> Variants { get; set; } = new List<ProductVariantPublicDetailDto>();

        // === THÔNG TIN MỚI (CHỈ NGƯỜI MUA CẦN) ===

        /// <summary>
        /// </summary>
        public ShopInfoForProductDto? ShopInfo { get; set; }

    }
}
