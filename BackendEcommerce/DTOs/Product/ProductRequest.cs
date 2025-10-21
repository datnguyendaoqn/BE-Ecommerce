namespace BackendEcommerce.DTOs.Product
{
    public class ProductRequest
    {
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Description { get; set; }
    }
}
