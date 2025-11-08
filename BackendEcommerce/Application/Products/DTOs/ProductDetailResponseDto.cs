namespace BackendEcommerce.Application.Products.DTOs
{
    public class ProductDetailResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Brand { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int ShopId { get; set; }
        public string Status { get; set; } = "active";

      
        public string? PrimaryImageUrl { get; set; }

        /// <summary>
        /// Danh sách TẤT CẢ các ảnh gallery (IsPrimary=false) của Product (cha)
        /// (Phần này vẫn giữ, vì DTO "Tạo" cho phép upload List<IFormFile> cho cha)
        /// </summary>
        public List<string> GalleryImageUrls { get; set; } = new List<string>();

        /// <summary>
        /// Danh sách TẤT CẢ các Biến thể (Variants)
        /// </summary>
        public List<ProductVariantDetailDto> Variants { get; set; } = new List<ProductVariantDetailDto>();
    }
    public class ProductVariantDetailDto
    {
        public int Id { get; set; }
        public string SKU { get; set; } = null!;
        public string? VariantSize { get; set; }
        public string? Color { get; set; }
        public string? Material { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        /// <summary>
        /// Ảnh đại diện duy nhất của Variant (con)
        /// </summary>
        public string? PrimaryImageUrl { get; set; }

        // (Như đã thống nhất, Variant (con) không có "Gallery")
    }
}
