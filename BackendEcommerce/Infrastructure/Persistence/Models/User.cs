namespace BackendEcommerce.Infrastructure.Persistence.Models
{
    /// <summary>
    /// Users table: stores application users.
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string Role { get; set; } = "customer";
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        // Navigation properties
        public ICollection<Account> Accounts { get; set; } = new List<Account>();
        public ICollection<AddressBook> AddressBooks { get; set; } = new List<AddressBook>();
        public ICollection<Shop> Shops { get; set; } = new List<Shop>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<DeliveryReview> DeliveryReviews { get; set; } = new List<DeliveryReview>();
        public ICollection<Cart> Carts { get; set; } = new List<Cart>();
       
    }
}