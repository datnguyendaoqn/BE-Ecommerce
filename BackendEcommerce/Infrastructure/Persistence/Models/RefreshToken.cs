namespace BackendEcommerce.Infrastructure.Persistence.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string Token { get; set; } = null!;
        public DateTime Expires { get; set; }
        public int RevokedValue { get; set; }  // NUMBER(1) trong Oracle
        public bool Revoked
        {
            get => RevokedValue == 1;
            set => RevokedValue = value ? 1 : 0;
        }
        public DateTime CreatedAt { get; set; }
        public string? CreatedByIp { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string? RevokedByIp { get; set; }
        public Account Account { get; set; } = null!;
        public bool IsExpired => DateTime.UtcNow >= Expires;
        public bool IsActive => !Revoked && !IsExpired;
    }
}
