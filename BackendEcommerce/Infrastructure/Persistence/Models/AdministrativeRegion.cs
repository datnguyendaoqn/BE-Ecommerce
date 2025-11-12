namespace BackendEcommerce.Infrastructure.Persistence.Models
{
    public class AdministrativeRegion
    {
        public int Id { get; set; } // (PK)
        public string Name { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string? CodeName { get; set; }
        public string? CodeNameEn { get; set; }

        // Navigation property (Thuộc tính điều hướng)
        public ICollection<Province> Provinces { get; set; } = new List<Province>();
    }
}
