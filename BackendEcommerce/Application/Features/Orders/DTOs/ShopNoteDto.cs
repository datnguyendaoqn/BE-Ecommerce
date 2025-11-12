using System.ComponentModel.DataAnnotations;

namespace BackendEcommerce.Application.Features.Orders.DTOs
{
    /// <summary>
    /// DTO (Đối tượng Truyền dữ liệu) (DTO (Đối tượng Truyền dữ liệu)) "con" (child) (mới)
    /// (Dùng để FE (Frontend) (Giao diện) (Giao diện) gửi (send) Ghi chú (Note) (Note) riêng (separate) cho từng Cửa hàng (Shop) (Shop))
    /// </summary>
    public class ShopNoteDto
    {
        [Required]
        public int ShopId { get; set; }

        [StringLength(500)]
        public string? Note { get; set; }
    }
}
