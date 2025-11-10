namespace BackendEcommerce.Application.Features.Products.DTOs
{
    public class CreateProductResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int ShopId { get; set; }
        public string? ProductImageUrl { get; set; } 
        public int VariantCount { get; set; }
        public List<CreateVariantResponseDto> Variants { get; set; } = new List<CreateVariantResponseDto>();
    }
    public class CreateVariantResponseDto
    {
        public int Id { get; set; }
        public string SKU { get; set; } = null!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Material { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
    }
}
