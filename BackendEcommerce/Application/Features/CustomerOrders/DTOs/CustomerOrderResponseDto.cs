namespace BackendEcommerce.Application.Features.CustomerOrders.DTOs
{
    /// <summary>
    /// DTO hiển thị danh sách đơn hàng cho Customer (Lịch sử mua hàng)
    /// </summary>
    public class CustomerOrderResponseDto
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public string ShopName { get; set; } // (Sẽ thêm ở Bước 3)
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int TotalItemsCount { get; set; }

        // Customer không cần biết chi tiết Ship (vì họ đã nhập),
        public List<CustomerOrderItemDto> Items { get; set; } = new List<CustomerOrderItemDto>();
    }
    /// <summary>
    /// DTO hiển thị item con bên trong lịch sử mua hàng
    /// </summary>
    public class CustomerOrderItemDto
    {
        public string ProductName { get; set; } = string.Empty;
        public string VariantName { get; set; } = string.Empty; // "Size L, Màu Đen"
        public string Sku { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; } // PriceAtTimeOfPurchase
        public string ImageUrl { get; set; } = string.Empty;
    }
}
