using System.ComponentModel.DataAnnotations;

namespace BackendEcommerce.Application.Features.SellerOrders.DTOs
{
    /// <summary>
    /// DTO cho yêu cầu cập nhật trạng thái đơn hàng từ Seller.
    /// Chỉ chứa trạng thái mới mà Seller muốn chuyển đến.
    /// </summary>
    public class SellerUpdateOrderStatusRequestDto
    {
        [Required(ErrorMessage = "Trạng thái mới không được để trống")]
        public string NewStatus { get; set; } = string.Empty;
    }
}
