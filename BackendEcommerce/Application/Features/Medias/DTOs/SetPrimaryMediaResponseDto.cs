namespace BackendEcommerce.Application.Features.Medias.DTOs
{
    public class SetPrimaryMediaResponseDto
    {
        public int ProductId { get; set; }
        public int NewPrimaryMediaId { get; set; }
        public string Message { get; set; } = "Đổi ảnh thumbnail thành công.";
    }
}
