using BackendEcommerce.Application.Features.Locations.DTOs;

namespace BackendEcommerce.Application.Features.Locations.Contracts
{
    public interface ILocationRepository
    {
        Task<List<LocationDto>> GetProvincesAsync();
        Task<List<LocationDto>> GetDistrictsByProvinceCodeAsync(string provinceCode);
        Task<List<LocationDto>> GetWardsByDistrictCodeAsync(string districtCode);
    }
}
