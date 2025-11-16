using BackendEcommerce.Application.Features.CustomerOrders.DTOs;
using BackendEcommerce.Application.Shared.DTOs;

namespace BackendEcommerce.Application.Features.CustomerOrders.Contracts
{
    /// <summary>
    /// Interface định nghĩa các nghiệp vụ Quản lý Đơn hàng
    /// CHỈ DÀNH RIÊNG cho vai trò Customer (Người mua).
    /// </summary>
    public interface ICustomerOrderService
    {
        /// <summary>
        /// Lấy lịch sử mua hàng của Customer (đã đăng nhập).
        /// </summary>
        /// <param name="userId">UserId của Customer</param>
        /// <param name="filter">Các tiêu chí lọc (status, phân trang...)</param>
        Task<PagedListResponseDto<CustomerOrderResponseDto>> GetMyOrdersAsync(int userId, CustomerOrderFilterDto filter);

        /// <summary>
        /// (MỚI) Lấy CHI TIẾT một đơn hàng
        /// </summary>
        /// <param name="userId">UserId của Customer (để xác thực)</param>
        /// <param name="orderId">ID của đơn hàng cần xem</param>
        /// <returns>DTO chi tiết đơn hàng</returns>
        Task<CustomerOrderDetailResponseDto> GetMyOrderDetailAsync(int userId, int orderId);

        /// <summary>
        /// Customer HỦY một đơn hàng đang ở trạng thái 'Pending'.
        /// </summary>
        /// <param name="userId">UserId của Customer (để xác thực)</param>
        /// <param name="orderId">ID của đơn hàng cần hủy</param>
        /// <returns>Đơn hàng đã được cập nhật (status: "Cancelled")</returns>
        Task<CustomerOrderResponseDto> CancelMyPendingOrderAsync(int userId, int orderId, CustomerCancelOrderRequestDto dto);
        /// <summary>
        /// Customer XÁC NHẬN đã nhận được hàng (khi đơn ở 'Shipped').
        /// </summary>
        /// <param name="userId">UserId của Customer (để xác thực)</param>
        /// <param name="orderId">ID của đơn hàng</param>
        /// <returns>Đơn hàng đã được cập nhật (status: "Delivered")</returns>
        Task<CustomerOrderResponseDto> ConfirmMyDeliveryAsync(int userId, int orderId);
    }
}
