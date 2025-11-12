using BackendEcommerce.Application.Features.Locations.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace BackendEcommerce.Presentation.Controllers
{
    /// <summary>
    /// API CÔNG KHAI (Public) - Dùng cho "Dropdown phụ thuộc"
    /// (Tải Tỉnh/Thành, Quận/Huyện, Phường/Xã)
    /// </summary>
    [Route("api/locations")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _locationService;

        public LocationController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        /// <summary>
        /// Lấy toàn bộ Tỉnh/Thành phố
        /// </summary>
        /// <remarks>
        /// Trả về danh sách tất cả các tỉnh/thành phố có trong hệ thống.
        /// </remarks>
        /// <returns>Danh sách ProvinceDto</returns>
        /// <response code="200">Thành công, trả về danh sách tỉnh/thành phố</response>
        [HttpGet("provinces")]
        public async Task<IActionResult> GetProvinces()
        {
            var result = await _locationService.GetProvincesAsync();
            return Ok(result);
        }

        /// <summary>
        /// Lấy Quận/Huyện theo Tỉnh/Thành phố
        /// </summary>
        /// <param name="provinceCode">Mã tỉnh/thành phố</param>
        /// <remarks>
        /// Ví dụ: ?provinceCode=HN
        /// </remarks>
        /// <returns>Danh sách DistrictDto</returns>
        /// <response code="200">Thành công, trả về danh sách quận/huyện</response>
        /// <response code="400">provinceCode không được để trống</response>
        [HttpGet("districts")]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetDistricts([FromQuery] string provinceCode)
        {
            if (string.IsNullOrEmpty(provinceCode))
            {
                return BadRequest(new { Message = "provinceCode is required" });
            }
            var result = await _locationService.GetDistrictsByProvinceCodeAsync(provinceCode);
            return Ok(result);
        }

        /// <summary>
        /// Lấy Phường/Xã theo Quận/Huyện
        /// </summary>
        /// <param name="districtCode">Mã quận/huyện</param>
        /// <remarks>
        /// Ví dụ: ?districtCode=Q1
        /// </remarks>
        /// <returns>Danh sách WardDto</returns>
        /// <response code="200">Thành công, trả về danh sách phường/xã</response>
        /// <response code="400">districtCode không được để trống</response>
        [HttpGet("wards")]
        [ProducesResponseType(400)]
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
