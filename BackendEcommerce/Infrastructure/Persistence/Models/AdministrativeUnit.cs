namespace BackendEcommerce.Infrastructure.Persistence.Models
{
    public class AdministrativeUnit
    {
        public int Id { get; set; } // (PK)
        public string? FullName { get; set; }
        public string? FullNameEn { get; set; }
        public string? ShortName { get; set; }
        public string? ShortNameEn { get; set; }
        public string? CodeName { get; set; }
        public string? CodeNameEn { get; set; }

        // (Chúng ta có thể thêm Navigation properties (Thuộc tính điều hướng) ở đây nếu cần)
        // public ICollection<Province> Provinces { get; set; } = new List<Province>();
        // public ICollection<District> Districts { get; set; } = new List<District>();
        // public ICollection<Ward> Wards { get; set; } = new List<Ward>();
    }
}
