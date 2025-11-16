using BackendEcommerce.Application.Shared.DTOs;

namespace BackendEcommerce.Application.Features.CustomerOrders.DTOs
{
    /// <summary>
    /// DTO lọc đơn hàng cho Customer, kế thừa từ PaginationRequestDto
    /// </summary>
    public class CustomerOrderFilterDto : PaginationRequestDto
    {
        /// <summary>
        /// Lọc theo trạng thái (Pending, Shipped, Delivered...)
        /// (null = lấy tất cả)
        /// </summary>
        public string? Status { get; set; }
    }
}
