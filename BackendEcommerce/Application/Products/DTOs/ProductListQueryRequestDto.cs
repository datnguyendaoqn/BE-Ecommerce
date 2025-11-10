using BackendEcommerce.Application.Shared.DTOs;
namespace BackendEcommerce.Application.Products.DTOs

{
     /// <summary>
    /// DTO đầu vào (Input) "Query" cho API Lấy Danh sách Sản phẩm.
    /// Bao gồm Phân trang, Lọc, Sắp xếp, và Tìm kiếm.
    /// </summary>
    public class ProductListQueryRequestDto : PaginationRequestDto // Kế thừa DTO Phân trang
    {
        public ProductListQueryRequestDto() : base()
        {
        }
        // 1. Filtering (Lọc)
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public double? MinRating { get; set; } // (Ví dụ: 4.0 - Lọc từ 4 sao trở lên)

        // 2. Sorting (Sắp xếp)
        // Giá trị hợp lệ: "popular" (phổ biến), "newest" (mới nhất), 
        // "price_asc" (giá tăng), "price_desc" (giá giảm)
        public string? SortBy { get; set; }

        // 3. Searching (Tìm kiếm)
        public string? SearchTerm { get; set; } // (Tìm kiếm theo Tên sản phẩm)
    }
}
