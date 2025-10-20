using ECommerceApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceApp.Models
{
    /// <summary>
    /// Merchant shops owned by users.
    /// </summary>
    [Table("shops")]
    public class Shop
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("owner_id")]
        public int OwnerId { get; set; }

        [Required]
        [Column("name")]
        public string Name { get; set; } = null!;

        [Column("description")]
        public string? Description { get; set; }

        [Column("logo")]
        public string? Logo { get; set; }

        [Required]
        [Column("status")]
        public string Status { get; set; } = "active";

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(OwnerId))]
        public User Owner { get; set; } = null!;

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}