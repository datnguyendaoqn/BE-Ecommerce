using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceApp.Models
{
    /// <summary>
    /// Media records (images) related to different entities; EntityType used to indicate owner.
    /// </summary>
    [Table("media")]
    public class Media
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        // Example values for EntityType: "product", "variant", "shop", "user"
        [Column("entity_type")]
        public string? EntityType { get; set; }

        // Stores the id of the related entity; nullable because some media may be transient
        [Column("entity_id")]
        public int? EntityId { get; set; }

        [Column("image_url")]
        public string? ImageUrl { get; set; }

        [Column("public_id")]
        public string? PublicId { get; set; }

        [Required]
        [Column("is_primary")]
        public bool IsPrimary { get; set; }

        [Column("alt_text")]
        public string? AltText { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // Note: polymorphic navigation not modeled here; use EntityType + EntityId to resolve.
    }
}