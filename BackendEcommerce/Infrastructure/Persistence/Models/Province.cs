namespace BackendEcommerce.Infrastructure.Persistence.Models
{
    public class Province
    {
        public string Code { get; set; } = string.Empty; // (PK)
        public string Name { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? FullNameEn { get; set; }
        public string? CodeName { get; set; }

        // Khóa ngoại (Foreign Keys)
        public int? AdministrativeUnitId { get; set; }
        public int? AdministrativeRegionId { get; set; }

        // Navigation properties (Thuộc tính điều hướng)
        public AdministrativeUnit? AdministrativeUnit { get; set; }
        public AdministrativeRegion? AdministrativeRegion { get; set; }
        public ICollection<District> Districts { get; set; } = new List<District>();
    }
}
