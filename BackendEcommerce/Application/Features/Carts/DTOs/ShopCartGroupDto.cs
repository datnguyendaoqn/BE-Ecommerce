namespace BackendEcommerce.Application.Features.Carts.DTOs
{
    /// <summary>
    /// DTO (Đối tượng Truyền dữ liệu) (DTO (Đối tượng Truyền dữ liệu)) "con" (child) đại diện (represent) cho 1 Nhóm Cửa hàng (Shop Group)
    /// (Dùng bên trong GroupedCartResponseDto)
    /// </summary>
    public class ShopCartGroupDto
    {
        public int ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;

        /// <summary>
        /// Danh sách (List) các món hàng (item) thuộc Cửa hàng (Shop) này
        /// </summary>
        public List<CartItemSnapshotDto> Items { get; set; } = new List<CartItemSnapshotDto>();

        /// <summary>
        /// Tổng tiền (Total Price) (Chỉ cho Nhóm Cửa hàng (Shop Group) này)
        /// </summary>
        public decimal SubTotalPrice => Items.Sum(i => i.PriceAtTimeOfAdd * i.Quantity);
    }
}
