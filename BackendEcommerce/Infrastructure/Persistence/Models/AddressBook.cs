using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendEcommerce.Infrastructure.Persistence.Models
{
    /// <summary>
    /// User addresses for shipping/billing.
    /// </summary>
    [Table("address_book")]
    public class AddressBook
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("full_name")]
        public string? FullName { get; set; }

        [Column("phone")]
        public string? Phone { get; set; }

        [Column("address_line")]
        public string? AddressLine { get; set; }

        [Column("ward")]
        public string? Ward { get; set; }

        [Column("district")]
        public string? District { get; set; }

        [Column("city")]
        public string? City { get; set; }

        [Required]
        [Column("is_default")]
        public bool IsDefault { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;
    }
}