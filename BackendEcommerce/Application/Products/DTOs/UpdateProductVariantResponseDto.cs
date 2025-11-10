namespace BackendEcommerce.Application.Products.DTOs
{
    public class UpdateProductVariantResponseDto
    {
        public int ProductId { get; set; }
        public int VariantId { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string SKU { get; set; } = null!;
    }
}
