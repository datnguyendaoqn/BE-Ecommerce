using System.ComponentModel.DataAnnotations;

namespace BackendEcommerce.Models
{
    public class Account
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public int Role { get; set; } = 1; // 1=customer, 2=seller, 3=admin

        public User? User { get; set; }
    }
}
