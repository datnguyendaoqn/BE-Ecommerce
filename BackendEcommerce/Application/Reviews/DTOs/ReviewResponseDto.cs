namespace BackendEcommerce.Application.Reviews.DTOs
{
    public class ReviewResponseDto
    {
        public int Id { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }

        public string VariantInfo { get; set; } = string.Empty;

        /// <summary>
        /// LƯU Ý: Không có List<ImageUrls> - extend feature
        /// </summary>
    }
}
