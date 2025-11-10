namespace BackendEcommerce.Application.Features.Products.DTOs
{
    public class ProductSummaryResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? PrimaryImageUrl { get; set; }
        public decimal MinPrice { get; set; }

        public int VariantCount { get; set; }

        public string Status { get; set; } = "active";
      
        public int CategoryId { get; set; }
        /// <summary>
        /// Tổng số lượt đã bán
        /// </summary>
        public int SelledCount { get; set; } = 0;

        /// <summary>
        /// Tổng số lượt đánh giá
        /// </summary>
        public int ReviewCount { get; set; } = 0;

        /// <summary>
        /// Điểm xếp hạng trung bình
        /// </summary>
        public double AverageRating { get; set; } = 0;

        public string CategoryName { get; set; } = string.Empty;
    }
}
