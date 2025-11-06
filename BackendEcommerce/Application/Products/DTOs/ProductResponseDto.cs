namespace BackendEcommerce.Application.Products.DTOs
{
    public class ProductResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int ShopId { get; set; }
        public List<VariantResponseDto> Variants { get; set; } = new List<VariantResponseDto>();
        public string? ProductImageUrl { get; set; } 
        public int VariantCount { get; set; }
    }
}
