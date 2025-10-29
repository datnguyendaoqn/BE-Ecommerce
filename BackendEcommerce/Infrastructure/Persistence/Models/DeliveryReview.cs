
namespace BackendEcommerce.Infrastructure.Persistence.Models
{
    /// <summary>
    /// Reviews for delivery experience per order.
    /// </summary>
    public class DeliveryReview
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int OrderId { get; set; }
        public decimal Rating { get; set; }
        public string? CommentText { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public User User { get; set; } = null!;
        public Order Order { get; set; } = null!;
    }
}