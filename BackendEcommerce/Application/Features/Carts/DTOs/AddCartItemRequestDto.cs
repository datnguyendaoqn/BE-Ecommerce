using System.ComponentModel.DataAnnotations;

namespace BackendEcommerce.Application.Features.Carts.DTOs
{
    public class AddCartItemRequestDto
    {
        [Required]
        public int ProductVariantId { get; set; }

        [Required]
        [Range(1, 100)] // (Giả định giới hạn 100)
        public int Quantity { get; set; }
    }
}
