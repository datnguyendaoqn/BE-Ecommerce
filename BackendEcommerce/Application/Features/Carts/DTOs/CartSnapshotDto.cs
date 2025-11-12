namespace BackendEcommerce.Application.Features.Carts.DTOs
{
    /// <summary>
    /// DTO "Vỏ bọc" (Wrapper) đại diện cho TOÀN BỘ GIỎ HÀNG
    /// (Đây là đối tượng JSON chính lưu trong Redis
    /// và cũng là thứ chúng ta trả về (return) trong API)
    /// </summary>
    public class CartSnapshotDto
    {
        public List<CartItemSnapshotDto> Items { get; set; } = new List<CartItemSnapshotDto>();

        /// <summary>
        /// Tổng số lượng (ví dụ: 5 món)
        /// (Tính toán động - dynamic)
        /// </summary>
        public int TotalItemsCount => Items.Sum(i => i.Quantity);

        /// <summary>
        /// Tổng tiền (ví dụ: 300.000đ)
        /// (Tính toán động - dynamic)
        /// </summary>
        public decimal TotalPrice => Items.Sum(i => i.PriceAtTimeOfAdd * i.Quantity);
    }
}
