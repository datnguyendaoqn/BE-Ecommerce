namespace BackendEcommerce.Infrastructure.Persistence.Models
{
    /// <summary>
    /// Orders placed by users.
    /// </summary>
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Status { get; set; } = "pending";
        public decimal? Total { get; set; }
        public int? ShippingAddressId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public User User { get; set; } = null!;
        public AddressBook? ShippingAddress { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<DeliveryReview> DeliveryReviews { get; set; } = new List<DeliveryReview>();
    }
}