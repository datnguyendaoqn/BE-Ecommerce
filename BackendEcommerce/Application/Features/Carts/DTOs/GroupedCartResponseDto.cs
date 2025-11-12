namespace BackendEcommerce.Application.Features.Carts.DTOs
{
    /// <summary>
    /// DTO (Đối tượng Truyền dữ liệu) (DTO (Đối tượng Truyền dữ liệu)) "Đầu ra" (Response) (Phản hồi) (Response) (Phản hồi) MỚI cho API (Giao diện Lập trình Ứng dụng) [GET] /api/cart
    /// (Đã được "Gom nhóm" (Grouped) theo ShopId (ID Cửa hàng) cho FE (Frontend) (Giao diện) (Giao diện))
    /// </summary>
    public class GroupedCartResponseDto
    {
        /// <summary>
        /// Danh sách (List) các Nhóm Cửa hàng (Shop Group)
        /// </summary>
        public List<ShopCartGroupDto> Shops { get; set; } = new List<ShopCartGroupDto>();

        /// <summary>
        /// TỔNG TIỀN (Grand Total) (của *tất cả* các Cửa hàng (Shop))
        /// </summary>
        public decimal GrandTotalPrice => Shops.Sum(s => s.SubTotalPrice);

        /// <summary>
        /// TỔNG SỐ LƯỢNG (Grand Total) (của *tất cả* các Cửa hàng (Shop))
        /// </summary>
        public int GrandTotalItemsCount => Shops.Sum(s => s.Items.Sum(i => i.Quantity));
    }
}
