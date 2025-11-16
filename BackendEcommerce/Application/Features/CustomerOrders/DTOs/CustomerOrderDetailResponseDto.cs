namespace BackendEcommerce.Application.Features.CustomerOrders.DTOs
{
    /// <summary>
    /// DTO hiển thị CHI TIẾT một đơn hàng cho Customer.
    /// Chứa đầy đủ thông tin (bao gồm cả địa chỉ đã snapshot).
    /// </summary>
    public class CustomerOrderDetailResponseDto
    {
        public int Id { get; set; } // Mã đơn hàng
        public int ShopId { get; set; }
        public string ShopName { get; set; } // (Sẽ thêm sau)
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // --- Thông tin chi tiết Địa chỉ (Snapshot) ---
        public string ShippingName { get; set; } = string.Empty;
        public string ShippingPhone { get; set; } = string.Empty;
        public string ShippingAddressLine { get; set; } = string.Empty;
        public string ShippingWard { get; set; } = string.Empty;
        public string ShippingDistrict { get; set; } = string.Empty;
        public string ShippingCity { get; set; } = string.Empty;
        public string? ShippingNote { get; set; }
        public string? CancellationReason { get; set; }

        // --- Danh sách sản phẩm chi tiết ---
        public List<CustomerOrderItemDto> Items { get; set; } = new List<CustomerOrderItemDto>();
    }
}
