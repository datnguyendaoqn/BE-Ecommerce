using System.ComponentModel.DataAnnotations;

namespace BackendEcommerce.Application.Features.AddressBooks.DTOs
{
    /// <summary>
    /// DTO "Ghi" (Write) - Dùng cho [POST] /api/addresses
    /// (FE (Frontend) chỉ cần gửi 3 Mã (Code))
    /// </summary>
    public class CreateAddressBookRequestDto
    {
        [Required]
        [StringLength(255)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string AddressLine { get; set; } = string.Empty; // (Số nhà)

        [Required]
        [StringLength(20)]
        public string ProvinceCode { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string DistrictCode { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string WardCode { get; set; } = string.Empty;

        public bool IsDefault { get; set; } = false;
    }
}
