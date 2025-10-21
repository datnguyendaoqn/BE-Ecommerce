using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendEcommerce.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Shop")]
        public int? ShopId { get; set; }

        [Required, MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
        public string? Brand { get; set; }

        [ForeignKey("Category")]
        public int? CategoryId { get; set; }

        public int Status { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public Shop? Shop { get; set; }
        public Category? Category { get; set; }

        public ICollection<ProductVariant>? Variants { get; set; }
    }
}
