using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendEcommerce.Infrastructure.Persistence.Models
{
    [Table("refresh_tokens")]
    public class RefreshToken
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("account_id")]
        public int AccountId { get; set; }

        [Required]
        [Column("token")]
        public string Token { get; set; } = null!;

        [Required]
        [Column("expires")]
        public DateTime Expires { get; set; }

        [Column("revoked")]
        public int RevokedValue { get; set; }  // NUMBER(1) trong Oracle

        [NotMapped]
        public bool Revoked
        {
            get => RevokedValue == 1;
            set => RevokedValue = value ? 1 : 0;
        }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("created_by_ip")]
        public string? CreatedByIp { get; set; }

        [Column("revoked_at")]
        public DateTime? RevokedAt { get; set; }

        [Column("revoked_by_ip")]
        public string? RevokedByIp { get; set; }

        [ForeignKey(nameof(AccountId))]
        public Account Account { get; set; } = null!;

        [NotMapped]
        public bool IsExpired => DateTime.UtcNow >= Expires;

        [NotMapped]
        public bool IsActive => !Revoked && !IsExpired;
    }
}
