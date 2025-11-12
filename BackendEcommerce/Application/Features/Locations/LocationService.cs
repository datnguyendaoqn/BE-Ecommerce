using BackendEcommerce.Application.Features.Locations.Contracts;
using BackendEcommerce.Application.Features.Locations.DTOs;
using BackendEcommerce.Application.Shared.DTOs;

namespace BackendEcommerce.Application.Features.Locations
{
    public class LocationService : ILocationService
    {
        private readonly ILocationRepository _locationRepository;

        public LocationService(ILocationRepository locationRepository)
        {
            _locationRepository = locationRepository;
        }

        public async Task<ApiResponseDTO<List<LocationDto>>> GetProvincesAsync()
        {
            var data = await _locationRepository.GetProvincesAsync();
            return new ApiResponseDTO<List<LocationDto>> { IsSuccess = true, Data = data };
        }

        public async Task<ApiResponseDTO<List<LocationDto>>> GetDistrictsByProvinceCodeAsync(string provinceCode)
        {
            var data = await _locationRepository.GetDistrictsByProvinceCodeAsync(provinceCode);
            return new ApiResponseDTO<List<LocationDto>> { IsSuccess = true, Data = data };
        }

        public async Task<ApiResponseDTO<List<LocationDto>>> GetWardsByDistrictCodeAsync(string districtCode)
        {
            var data = await _locationRepository.GetWardsByDistrictCodeAsync(districtCode);
            return new ApiResponseDTO<List<LocationDto>> { IsSuccess = true, Data = data };
        }
    }
}
