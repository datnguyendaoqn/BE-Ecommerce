using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Principal;

namespace BackendEcommerce.Models
{
    public class User
    {
        [Key, ForeignKey("Account")]
        public int Id { get; set; }

        [Required, MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        public string? FullName { get; set; }
        public string? Phone { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public Account? Account { get; set; }

        public ICollection<AddressBook>? AddressBooks { get; set; }
        public ICollection<Shop>? Shops { get; set; }
        public ICollection<Cart>? Carts { get; set; }
        public ICollection<Order>? Orders { get; set; }
        public ICollection<Review>? Reviews { get; set; }
    }
}
