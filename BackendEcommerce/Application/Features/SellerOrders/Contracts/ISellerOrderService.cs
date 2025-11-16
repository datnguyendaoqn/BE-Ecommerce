using BackendEcommerce.Application.Features.Orders.DTOs;
using BackendEcommerce.Application.Features.SellerOrders.DTOs;
using BackendEcommerce.Application.Shared.DTOs;

namespace BackendEcommerce.Application.Features.SellerOrders.Contracts
{
    /// <summary>
    /// Interface định nghĩa các nghiệp vụ Quản lý Đơn hàng
    /// CHỈ DÀNH RIÊNG cho vai trò Seller (Người bán).
    /// </summary>
    public interface ISellerOrderService
    {
        /// <summary>
        /// Lấy lịch sử đơn hàng của Shop (đã làm ở bước trước).
        /// </summary>
        /// <param name="userId">UserId của Seller đang đăng nhập</param>
        /// <param name="filter">Các tiêu chí lọc (status, phân trang...)</param>
        /// <returns>Danh sách đơn hàng đã phân trang</returns>
        Task<PagedListResponseDto<OrderSellerResponseDto>> GetShopOrdersAsync(int userId, OrderFilterDto filter);

        /// <summary>
        /// Cập nhật trạng thái của một đơn hàng.
        /// (Service sẽ chứa logic "State Machine" để validate việc chuyển đổi).
        /// </summary>
        /// <param name="userId">UserId của Seller đang đăng nhập (để tìm ShopId và xác thực)</param>
        /// <param name="orderId">ID của đơn hàng cần cập nhật</param>
        /// <param name="newStatus">Trạng thái mới muốn chuyển đến (vd: "Processing", "Shipped")</param>
        /// <returns>Một đối tượng OrderSellerResponseDto đã được cập nhật</returns>
        Task<OrderSellerResponseDto> UpdateOrderStatusAsync(int userId, int orderId, string newStatus);
    }
}
