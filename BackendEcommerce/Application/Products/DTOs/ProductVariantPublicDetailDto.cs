using BackendEcommerce.Application.Medias.DTOs;

namespace BackendEcommerce.Application.Products.DTOs
{
    public class ProductVariantPublicDetailDto
    {
        public int Id { get; set; }
        // (SKU đã bị ẩn)
        public string? VariantSize { get; set; }
        public string? Color { get; set; }
        public string? Material { get; set; }
        public decimal Price { get; set; }

        /// <summary>
        /// Chỉ báo "Còn hàng" (true) hay "Hết hàng" (false).
        /// Không lộ số lượng tồn kho chính xác.
        /// </summary>
        public bool IsInStock { get; set; }
        // (Quantity đã bị ẩn)

        public VariantMediaDto? PrimaryImage { get; set; }
    }
}
