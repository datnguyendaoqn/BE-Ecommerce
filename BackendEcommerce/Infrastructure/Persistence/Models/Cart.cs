namespace BackendEcommerce.Infrastructure.Persistence.Models
{
    /// <summary>
    /// Shopping carts belonging to users.
    /// </summary>
    public class Cart
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public User User { get; set; } = null!;

        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}