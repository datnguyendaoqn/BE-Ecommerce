using System.ComponentModel.DataAnnotations;

namespace BackendEcommerce.Application.Features.Products.DTOs
{
    public class AddVariantRequestDto
    {
        [Required]
        [StringLength(100)]
        public string SKU { get; set; } = null!;

        [StringLength(50)]
        public string? VariantSize { get; set; }

        [StringLength(50)]
        public string? Color { get; set; }

        [StringLength(100)]
        public string? Material { get; set; }

        [Required]
        [Range(1000, (double)decimal.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        /// <summary>
        /// Ảnh (duy nhất) của Variant
        /// </summary>
        [Required]
        public IFormFile Image { get; set; } = null!;
    }
}
