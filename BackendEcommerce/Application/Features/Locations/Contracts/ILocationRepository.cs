using BackendEcommerce.Application.Features.Locations.DTOs;

namespace BackendEcommerce.Application.Features.Locations.Contracts
{
    public interface ILocationRepository
    {
        Task<List<LocationDto>> GetProvincesAsync();
        Task<List<LocationDto>> GetDistrictsByProvinceCodeAsync(string provinceCode);
        Task<List<LocationDto>> GetWardsByDistrictCodeAsync(string districtCode);
        // === BỔ SUNG (CHO "SNAPSHOT" (SAO CHÉP) CỦA ADDRESSBOOK) ===

        /// <summary>
        /// (Mới) Lấy 1 Tỉnh/Thành (để "Snapshot" (Sao chép) Tên (Name))
        /// </summary>
        Task<LocationDto?> GetProvinceByCodeAsync(string provinceCode);

        /// <summary>
        /// (Mới) Lấy 1 Quận/Huyện (để "Snapshot" (Sao chép) Tên (Name))
        /// </summary>
        Task<LocationDto?> GetDistrictByCodeAsync(string districtCode);

        /// <summary>
        /// (Mới) Lấy 1 Phường/Xã (để "Snapshot" (Sao chép) Tên (Name))
        /// </summary>
        Task<LocationDto?> GetWardByCodeAsync(string wardCode);
    }
}
