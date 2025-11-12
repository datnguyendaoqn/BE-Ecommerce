namespace BackendEcommerce.Application.Features.Orders.DTOs
{
    public class OrderSellerResponseDto
    {
        public int Id { get; set; }

        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Thông tin người nhận (Snapshot) - Seller cần biết để ship
        public string ShippingName { get; set; } = string.Empty;
        public string ShippingPhone { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty; // Full address ghép từ các trường snapshot

        // Danh sách sản phẩm trong đơn (để hiển thị nhanh)
        public List<OrderItemSellerDto> Items { get; set; } = new List<OrderItemSellerDto>();
    }

    public class OrderItemSellerDto
    {
        public string ProductName { get; set; } = string.Empty;
        public string VariantName { get; set; } = string.Empty; // "Size L, Màu Đen"
        public string Sku { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; } // PriceAtTimeOfPurchase
        public string ImageUrl { get; set; } = string.Empty;
    }
}
