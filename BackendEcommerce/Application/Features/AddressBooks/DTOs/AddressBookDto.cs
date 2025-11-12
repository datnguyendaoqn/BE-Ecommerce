namespace BackendEcommerce.Application.Features.AddressBooks.DTOs
{
    /// <summary>
    /// DTO "Đọc" (Read) - Dùng để trả về (return) cho FE
    /// (Đã "Snapshot" (Sao chép) 6 trường, không cần JOIN)
    /// </summary>
    public class AddressBookDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;

        // Dữ liệu "Snapshot" (Sao chép) (Tên)
        public string ProvinceName { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
        public string WardName { get; set; } = string.Empty;

        // Dữ liệu ID (Mã) (FE (Frontend) không cần, nhưng trả về (return) cũng OK)
        public string ProvinceCode { get; set; } = string.Empty;
        public string DistrictCode { get; set; } = string.Empty;
        public string WardCode { get; set; } = string.Empty;

        public bool IsDefault { get; set; }
    }
}
