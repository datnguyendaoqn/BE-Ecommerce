namespace BackendEcommerce.Application.Medias.DTOs
{
    public class ProductMediaDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }
    public class VariantMediaDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }

}
