namespace BackendEcommerce.Application.Features.Products.DTOs
{
    public class RelatedProductsResponseDto
    {
        /// <summary>
        /// Danh sách các sản phẩm khác từ CÙNG MỘT SHOP
        /// </summary>
        public List<ProductCardDto> SameShopProducts { get; set; } = new List<ProductCardDto>();

        // --- ĐÃ XÓA ---
        // public List<ProductCardDto> SameCategoryProducts { get; set; } = new List<ProductCardDto>();
    }
}
