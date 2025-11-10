using System.ComponentModel.DataAnnotations;

namespace BackendEcommerce.Application.Features.Products.DTOs
{
    public class UpdateProductRequestDto
    {
        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        [StringLength(255, ErrorMessage = "Tên sản phẩm không được vượt quá 255 ký tự")]
        public string Name { get; set; } = null!;

        [StringLength(2000, ErrorMessage = "Mô tả không được vượt quá 2000 ký tự")]
        public string? Description { get; set; }

        [StringLength(100, ErrorMessage = "Thương hiệu không được vượt quá 100 ký tự")]
        public string? Brand { get; set; }

        [Required(ErrorMessage = "Category là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "CategoryId không hợp lệ")]
        public int CategoryId { get; set; }
    }
}
