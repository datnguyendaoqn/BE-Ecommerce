using CloudinaryDotNet;
using Microsoft.EntityFrameworkCore;

namespace BackendEcommerce.Application.Products.DTOs
{
    public class ProductCardDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// </summary>
        public string PrimaryImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// </summary>
        public decimal MinPrice { get; set; }

        /// <summary>
        /// </summary>
        public double AverageRating { get; set; } = 0;

        /// <summary>
        /// </summary>
        public int ReviewCount { get; set; } = 0;

        /// <summary>
        /// Tổng số lượt đã bán (ví dụ: 1500)
        /// </summary>
        public int SelledCount { get; set; } = 0;
    }
}

