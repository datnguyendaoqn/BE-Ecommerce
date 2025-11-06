using System.ComponentModel.DataAnnotations;

namespace BackendEcommerce.Application.Products.DTOs
{
    public class CreateVariantDto
    {
        [Required]
        public string SKU { get; set; } = null!;
        public string? VariantSize { get; set; }
        public string? Color { get; set; }

        [Required]
        [Range(1000, 999999999, ErrorMessage = "Số tiền phải lớn hơn 1000 VND.")]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        // The image file for THIS variant
        [Required(ErrorMessage = "Sản phẩm phải có hình")]
        public IFormFile Image { get; set; } = null!;
    }
}
