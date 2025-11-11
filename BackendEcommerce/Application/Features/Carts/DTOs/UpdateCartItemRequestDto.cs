using System.ComponentModel.DataAnnotations;

namespace BackendEcommerce.Application.Features.Carts.DTOs
{
    public class UpdateCartItemRequestDto
    {
        [Required]
        public int ProductVariantId { get; set; }

        [Required]
        [Range(1, 100)] // (Giả định FE sẽ gọi API [DELETE] nếu User gõ 0)
        public int NewQuantity { get; set; }
    }
}
