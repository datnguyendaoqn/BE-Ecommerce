namespace BackendEcommerce.Infrastructure.Persistence.Models
{
    /// <summary>
    /// User addresses for shipping/billing.
    /// </summary>
    public class AddressBook
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string AddressLine { get; set; } // (Số nhà, Tên đường)

        // === BẮT ĐẦU THAY ĐỔI (Kiến trúc "Snapshot" 6 trường) ===

        // 1. Dữ liệu ID (Chuẩn hóa) - Dùng để Validate & Tính phí Ship
        public string ProvinceCode { get; set; } = string.Empty;
        public string DistrictCode { get; set; } = string.Empty;
        public string WardCode { get; set; } = string.Empty;

        // 2. Dữ liệu "Snapshot" (Sao chép) Tên (Name) - Dùng để Hiển thị (Display)
        // (Giúp API [GET /api/addresses] siêu nhanh, không cần JOIN)
        public string ProvinceName { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
        public string WardName { get; set; } = string.Empty;

        // === KẾT THÚC THAY ĐỔI ===

        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
    }
}