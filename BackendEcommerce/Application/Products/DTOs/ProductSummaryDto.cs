namespace BackendEcommerce.Application.Products.DTOs
{
    public class ProductSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? PrimaryImageUrl { get; set; }
        public decimal MinPrice { get; set; }

        public int VariantCount { get; set; }

        public string Status { get; set; } = "active";
    }
}
