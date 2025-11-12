namespace BackendEcommerce.Application.Features.Orders.DTOs
{
    /// <summary>
    /// DTO "Nhẹ" (Lightweight) Trả về (Return) sau khi Đặt hàng (Order) thành công
    /// (ĐÃ CẬP NHẬT (UPDATED): Để hỗ trợ (support) "Tách Đơn hàng (Order)" (Split Order) Đa Cửa hàng (Multi-Shop))
    /// </summary>
    public class CreateOrderResponseDto
    {
        // [THAY ĐỔI] Từ int -> List<int>
        /// <summary>
        /// Danh sách (List) các ID (Mã) Đơn hàng (Order) (cha) đã được tạo (created)
        /// (Mỗi ID (Mã) cho 1 Cửa hàng (Shop))
        /// </summary>
        public List<int> CreatedOrderIds { get; set; } = new List<int>();

        public DateTime CreatedAt { get; set; } // (Thời gian (Time) tạo (create) Đơn hàng (Order) (Order) đầu tiên)

        // (Chúng ta (backend) (bạn và tôi) bỏ (remove) Status (Trạng thái) và TotalAmount (Tổng tiền)
        //  vì giờ đây có nhiều (multiple) Đơn hàng (Order) (Order) với nhiều (multiple) Status (Trạng thái)/Totals (Tổng))
    }
}
