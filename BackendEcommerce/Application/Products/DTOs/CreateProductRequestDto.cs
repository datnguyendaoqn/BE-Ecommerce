using System.ComponentModel.DataAnnotations;

namespace BackendEcommerce.Application.Products.DTOs
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
        public List<CreateVariantDto> Variants { get; set; } = new List<CreateVariantDto>();

        // We will send product-level images (optional)
        public IFormFile? ProductImages { get; set; }
    }
}
