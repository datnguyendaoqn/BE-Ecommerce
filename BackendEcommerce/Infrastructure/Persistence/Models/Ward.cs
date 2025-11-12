namespace BackendEcommerce.Infrastructure.Persistence.Models
{
    public class Ward
    {
        public string Code { get; set; } = string.Empty; // (PK)
        public string Name { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public string? FullName { get; set; }
        public string? FullNameEn { get; set; }
        public string? CodeName { get; set; }

        // Khóa ngoại (Foreign Keys)
        public string? DistrictCode { get; set; }
        public int? AdministrativeUnitId { get; set; }

        // Navigation properties (Thuộc tính điều hướng)
        public District? District { get; set; }
        public AdministrativeUnit? AdministrativeUnit { get; set; }
    }
}
