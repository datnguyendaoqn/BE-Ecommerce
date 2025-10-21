using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendEcommerce.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public Category? Parent { get; set; }

        public ICollection<Category>? SubCategories { get; set; }
        public ICollection<Product>? Products { get; set; }
    }
}
