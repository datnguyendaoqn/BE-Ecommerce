using System.ComponentModel.DataAnnotations;

namespace BackendEcommerce.Application.Features.Reviews.DTOs
{
    public class CreateReviewRequestDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int OrderItemId { get; set; } // Món hàng cụ thể trong đơn hàng

        [Required]
        [Range(1, 5)] // Rating từ 1 đến 5 sao
        public decimal Rating { get; set; }

        public string? CommentText { get; set; }

    }
}
