using System.ComponentModel.DataAnnotations;

namespace BackendEcommerce.Application.Products.DTOs
{
    public class UpdateProductVariantRequestDto
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string SKU { get; set; } = null!;

        [StringLength(50)]
        public string? VariantSize { get; set; }

        [StringLength(50)]
        public string? Color { get; set; }

        [StringLength(100)]
        public string? Material { get; set; }

        [Required]
        [Range(0, (double)decimal.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
