namespace BackendEcommerce.Infrastructure.Persistence.Models
{
    /// <summary>
    /// Product variant reviews by users.
    /// </summary>
    public class Review
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int VariantId { get; set; }
        public decimal Rating { get; set; }
        public string? CommentText { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public User User { get; set; } = null!;
        public ProductVariant Variant { get; set; } = null!;
    }
}