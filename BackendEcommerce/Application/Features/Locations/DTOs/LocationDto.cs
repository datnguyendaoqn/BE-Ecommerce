namespace BackendEcommerce.Application.Features.Locations.DTOs
{
    public class LocationDto
    {
        /// <summary>
        /// DTO "siêu nhẹ" (Lightweight) chung cho Province, District, Ward
        /// (Dùng cho API "Dropdown phụ thuộc")
        /// </summary>
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
