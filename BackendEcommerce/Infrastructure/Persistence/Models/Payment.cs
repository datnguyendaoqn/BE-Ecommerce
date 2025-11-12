namespace BackendEcommerce.Infrastructure.Persistence.Models
{
    /// <summary>
    /// Payments related to orders.
    /// </summary>
    public class Payment
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string Method { get; set; } = null!;
        public string Status { get; set; } = "pending";
        public string? TransactionId { get; set; }
        public decimal Amount { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Order Order { get; set; } = null!;
    }
}