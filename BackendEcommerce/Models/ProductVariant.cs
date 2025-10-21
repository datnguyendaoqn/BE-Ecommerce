using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendEcommerce.Models
{
    public class ProductVariant
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }

        [Required, MaxLength(100)]
        public string Sku { get; set; } = string.Empty;

        public string? VariantSize { get; set; }
        public string? Color { get; set; }
        public string? Material { get; set; }

        [Required]
        public decimal Price { get; set; }

        public int Quantity { get; set; } = 0;

        public Product? Product { get; set; }

        public ICollection<CartItem>? CartItems { get; set; }
        public ICollection<OrderItem>? OrderItems { get; set; }
        public ICollection<Review>? Reviews { get; set; }
    }
}
