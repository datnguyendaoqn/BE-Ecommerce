using BackendEcommerce.Application.Features.Medias.DTOs;

namespace BackendEcommerce.Application.Features.Products.DTOs
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
       
        public List<ProductMediaDto> ProductImages { get; set; } = new List<ProductMediaDto>();
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
        public bool IsInStock { get; set; }
        public VariantMediaDto? PrimaryImage { get; set; }
    }
}
