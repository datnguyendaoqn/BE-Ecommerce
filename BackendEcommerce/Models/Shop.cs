using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendEcommerce.Models
{
    public class Shop
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int? UserId { get; set; }

        [Required, MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        public string? Image { get; set; }
        public string? Description { get; set; }

        public int IsActive { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public User? User { get; set; }
        public ICollection<Product>? Products { get; set; }
    }
}
