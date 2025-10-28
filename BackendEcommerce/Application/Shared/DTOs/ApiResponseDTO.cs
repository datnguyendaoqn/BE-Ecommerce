namespace BackendEcommerce.Application.Shared.DTOs
{
    public class ApiResponseDTO<T>
    {
        public bool IsSuccess { get; set; }       // FE check nhanh
        public int? Code { get; set; }             // HTTP hoặc custom code
        public string? Message { get; set; } = ""; // Thông báo
        public T? Data { get; set; }              // Dữ liệu trả về, generic
    }
}
