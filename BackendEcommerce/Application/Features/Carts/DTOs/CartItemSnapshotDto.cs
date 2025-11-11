namespace BackendEcommerce.Application.Features.Carts.DTOs
{
    public class CartItemSnapshotDto
    {
        public int ProductVariantId { get; set; }
        public int Quantity { get; set; }

        // --- Dữ liệu "Snapshot" (Sao chép) ---
        // (Lấy từ ProductVariant tại thời điểm thêm vào giỏ)
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// Giá tại thời điểm thêm vào giỏ
        /// </summary>
        public decimal PriceAtTimeOfAdd { get; set; }
    }
}
