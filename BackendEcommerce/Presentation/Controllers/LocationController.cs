using BackendEcommerce.Application.Features.Locations.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace BackendEcommerce.Presentation.Controllers
{
    /// <summary>
    /// API CÔNG KHAI (Public) - Dùng cho "Dropdown phụ thuộc"
    /// (Tải Tỉnh/Thành, Quận/Huyện, Phường/Xã)
    /// </summary>
    [Route("api/location")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _locationService;

        public LocationController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        [HttpGet("provinces")]
        public async Task<IActionResult> GetProvinces()
        {
            var result = await _locationService.GetProvincesAsync();
            return Ok(result);
        }

        [HttpGet("districts")]
        public async Task<IActionResult> GetDistricts([FromQuery] string provinceCode)
        {
            // (Thêm Validation (Xác thực) cơ bản)
            if (string.IsNullOrEmpty(provinceCode))
            {
                return BadRequest(new { Message = "provinceCode is required" });
            }
            var result = await _locationService.GetDistrictsByProvinceCodeAsync(provinceCode);
            return Ok(result);
        }

        [HttpGet("wards")]
        public async Task<IActionResult> GetWards([FromQuery] string districtCode)
        {
            if (string.IsNullOrEmpty(districtCode))
            {
                return BadRequest(new { Message = "districtCode is required" });
            }
            var result = await _locationService.GetWardsByDistrictCodeAsync(districtCode);
            return Ok(result);
        }
    }
}
