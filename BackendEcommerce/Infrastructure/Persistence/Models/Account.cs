namespace BackendEcommerce.Infrastructure.Persistence.Models
{
    /// <summary>
    /// Accounts linked to users (credentials).
    /// </summary>
    public class Account
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        // **Refresh tokens**
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}