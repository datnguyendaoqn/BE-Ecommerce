namespace BackendEcommerce.Infrastructure.Persistence.Models
{
    /// <summary>
    /// Orders placed by users.
    /// </summary>
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Status { get; set; } = "pending"; // (ví dụ: pending, paid, shipped...)
        public decimal TotalAmount { get; set; } // (Thay cho Total, không nullable)

        public string PaymentMethod { get; set; } = "COD"; // (Bổ sung từ file Payment)

        public string? CancellationReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // === BẮT ĐẦU THAY ĐỔI (SNAPSHOT ĐỊA CHỈ) ===

        // (Bỏ: int? ShippingAddressId)
        // (Bỏ: AddressBook? ShippingAddress)

        // (Bổ sung) Sao chép địa chỉ tại thời điểm mua
        public string Shipping_FullName { get; set; } = string.Empty;
        public string Shipping_Phone { get; set; } = string.Empty;
        public string Shipping_AddressLine { get; set; } = string.Empty;
        public string Shipping_Ward { get; set; } = string.Empty;
        public string Shipping_District { get; set; } = string.Empty;
        public string Shipping_City { get; set; } = string.Empty;


        // Navigation properties
        public User User { get; set; } = null!;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();

        public ICollection<DeliveryReview> DeliveryReviews { get; set; } = new List<DeliveryReview>();
    }
}