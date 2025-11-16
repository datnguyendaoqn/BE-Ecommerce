using System.ComponentModel.DataAnnotations;

namespace BackendEcommerce.Application.Features.CustomerOrders.DTOs
{
    /// <summary>
    /// DTO chứa lý do hủy đơn từ Customer
    /// </summary>
    public class CustomerCancelOrderRequestDto
    {
        [MaxLength(500, ErrorMessage = "Lý do hủy không được vượt quá 500 ký tự")]
        public string? Reason { get; set; } 
    }
}
