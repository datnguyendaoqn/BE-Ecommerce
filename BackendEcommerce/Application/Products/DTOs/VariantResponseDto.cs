namespace BackendEcommerce.Application.Products.DTOs
{
    public class VariantResponseDto
    {
        public int Id { get; set; }
        public string SKU { get; set; } = null!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; } = null!;
    }
}
