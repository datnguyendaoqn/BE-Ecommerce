
namespace BackendEcommerce.Infrastructure.Persistence.Models
{
    /// <summary>
    /// Items in an order referencing product variants.
    /// </summary>
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int VariantId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Order Order { get; set; } = null!;
        public ProductVariant Variant { get; set; } = null!;
    }
}