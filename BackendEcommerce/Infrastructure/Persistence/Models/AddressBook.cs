namespace BackendEcommerce.Infrastructure.Persistence.Models
{
    /// <summary>
    /// User addresses for shipping/billing.
    /// </summary>
    public class AddressBook
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? AddressLine { get; set; }
        public string? Ward { get; set; }
        public string? District { get; set; }
        public string? City { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public User User { get; set; } = null!;
    }
}