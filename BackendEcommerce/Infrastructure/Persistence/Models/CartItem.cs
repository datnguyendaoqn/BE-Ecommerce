

namespace BackendEcommerce.Infrastructure.Persistence.Models
{
    /// <summary>
    /// Items inside a cart referencing a product variant.
    /// </summary>
    public class CartItem
    {
        public int Id { get; set; }

        public int CartId { get; set; }

        public int VariantId { get; set; }
        public int Quantity { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Cart Cart { get; set; } = null!;

        public ProductVariant Variant { get; set; } = null!;
    }
}