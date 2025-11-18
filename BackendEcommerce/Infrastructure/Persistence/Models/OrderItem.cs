
namespace BackendEcommerce.Infrastructure.Persistence.Models
{
    /// <summary>
    /// Items in an order referencing product variants.
    /// </summary>
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int Quantity { get; set; }
        // 1. (Đổi tên) Rõ ràng hơn: Đây là giá tại thời điểm mua
        public decimal PriceAtTimeOfPurchase { get; set; } // (Thay cho UnitPrice)

        // 2. (Bổ sung) Sao chép các thông tin cốt lõi
        public string Sku { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;

        // (ví dụ: "Size L, Màu Đen")
        public string VariantName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;

        // 3. (Thay đổi) Khóa ngoại Phải là Nullable
        //    Để hỗ trợ "On Delete Set Null"
        public int? ProductVariantId { get; set; } 

        // === KẾT THÚC THAY ĐỔI ===

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Order Order { get; set; } = null!;

        // (Thay đổi) Phải là Nullable
        public ProductVariant? Variant { get; set; } // (Thay cho ProductVariant)
    }
}