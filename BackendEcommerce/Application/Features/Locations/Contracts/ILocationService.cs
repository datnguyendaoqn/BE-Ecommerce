using BackendEcommerce.Application.Features.Locations.DTOs;
using BackendEcommerce.Application.Shared.DTOs;

namespace BackendEcommerce.Application.Features.Locations.Contracts
{
    public interface ILocationService
    {
        Task<ApiResponseDTO<List<LocationDto>>> GetProvincesAsync();
        Task<ApiResponseDTO<List<LocationDto>>> GetDistrictsByProvinceCodeAsync(string provinceCode);
        Task<ApiResponseDTO<List<LocationDto>>> GetWardsByDistrictCodeAsync(string districtCode);
    }
}
