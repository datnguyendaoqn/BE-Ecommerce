using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceApp.Models
{
    /// <summary>
    /// Payments related to orders.
    /// </summary>
    [Table("payments")]
    public class Payment
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("order_id")]
        public int OrderId { get; set; }

        [Required]
        [Column("method")]
        public string Method { get; set; } = null!;

        [Required]
        [Column("status")]
        public string Status { get; set; } = "pending";

        [Column("transaction_id")]
        public string? TransactionId { get; set; }

        [Required]
        [Column("amount")]
        public decimal Amount { get; set; }

        [Column("paid_at")]
        public DateTime? PaidAt { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order Order { get; set; } = null!;
    }
}