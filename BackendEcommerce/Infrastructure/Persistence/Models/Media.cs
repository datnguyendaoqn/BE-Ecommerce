namespace BackendEcommerce.Infrastructure.Persistence.Models
{
    /// <summary>
    /// Media records (images) related to different entities; EntityType used to indicate owner.
    /// </summary>
    public class Media
    {
        public int Id { get; set; }

        // Example values for EntityType: "product", "variant", "shop", "user"
        public string? EntityType { get; set; }

        // Stores the id of the related entity; nullable because some media may be transient
        public int? EntityId { get; set; }

        public string? ImageUrl { get; set; }

        public string? PublicId { get; set; }
        public bool IsPrimary { get; set; }
        public string? AltText { get; set; }
        public DateTime CreatedAt { get; set; }

        // Note: polymorphic navigation not modeled here; use EntityType + EntityId to resolve.
    }
}