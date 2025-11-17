using BackendEcommerce.Application.Features.CustomerOrders.DTOs;
using BackendEcommerce.Infrastructure.Persistence.Models;

namespace BackendEcommerce.Application.Features.Orders.Contracts
{
    public interface IOrderRepository
    {
        Task AddAsync(Order order);

        // (Chúng ta cần SaveChangesAsync riêng biệt
        //  để lấy OrderId (ID Đơn hàng) MỚI trước khi tạo (create) OrderItem)
        Task SaveChangesAsync();
        /// <summary>
        /// Lấy danh sách đơn hàng có phân trang cho một Shop cụ thể.
        /// </summary>
        /// <param name="shopId">ID của Shop</param>
        /// <param name="status">Filter theo trạng thái (null = lấy tất cả)</param>
        /// <param name="pageNumber">Số trang (bắt đầu từ 1)</param>
        /// <param name="pageSize">Số lượng item mỗi trang</param>
        /// <returns>Một Tuple chứa danh sách Order (đã Include Items) và Tổng số lượng (TotalCount)</returns>
        Task<(IEnumerable<Order> Orders, int TotalCount)> GetOrdersByShopIdAsync(
            int shopId,
            string? status,
            int pageNumber,
            int pageSize);
        /// <summary>
        /// (HÀM MỚI) Lấy một đơn hàng duy nhất bằng ID,
        /// và bao gồm cả các OrderItems liên quan.
        /// </summary>
        Task<Order?> GetOrderDetailByIdWithItemsAsync(int orderId);

        /// <summary>
        /// (HÀM MỚI) Đánh dấu một đơn hàng là đã bị sửa đổi (Modified).
        /// Hàm này không bất đồng bộ, nó chỉ thay đổi trạng thái của Entity trong DbContext.
        /// </summary>
        void Update(Order order);
        /// <summary>
        /// Sửa đổi: Hàm này giờ trả về DTO đã được tối ưu (Projection)
        /// thay vì trả về Entity (Order).
        /// </summary>
        Task<(IEnumerable<CustomerOrderResponseDto> Orders, int TotalCount)> GetOrdersByUserIdAsync(
            int userId,
            string? status,
            int pageNumber,
            int pageSize);
        // === (HÀM MỚI CHO REVIEW SERVICE) ===
        /// <summary>
        /// Lấy 1 OrderItem để kiểm tra nghiệp vụ Review.
        /// (Bao gồm cả Order (cha) và Variant (anh em) để check)
        /// </summary>
        Task<OrderItem?> GetOrderItemForReviewAsync(int orderItemId);


    }
}
