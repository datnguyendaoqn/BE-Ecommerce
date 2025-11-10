using System.ComponentModel.DataAnnotations;

namespace BackendEcommerce.Application.Features.Products.DTOs
{
    public class CreateProductRequestDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }
        public string? Brand { get; set; }

        [Required]
        public int CategoryId { get; set; }

        // We will send variants as a list
        [Required]
        public List<CreateVariantRequestDto> Variants { get; set; } = new List<CreateVariantRequestDto>();

        // We will send product-level images (optional)
        public List<IFormFile>? ProductImages { get; set; }
    }
    public class CreateVariantRequestDto
    {
        [Required]
        public string SKU { get; set; } = null!;
        [Required]
        public string? VariantSize { get; set; }
        [Required]
        public string? Color { get; set; }

        [Required]
        [Range(1000, 999999999, ErrorMessage = "Số tiền phải lớn hơn 1000 VND.")]
        public decimal Price { get; set; }
        [Required]
        public string Material { get; set; } = null!;

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        // The image file for THIS variant
        [Required(ErrorMessage = "Sản phẩm phải có hình")]
        public IFormFile Image { get; set; } = null!;
    }
}
