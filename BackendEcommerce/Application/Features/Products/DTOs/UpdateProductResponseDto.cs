using System.ComponentModel.DataAnnotations;

namespace BackendEcommerce.Application.Features.Products.DTOs
{
    public class UpdateProductResponseDto
    {
        public int Id { get; set; }
        public DateTime UpdatedAt { get; set; }
       
        public string CategoryName { get; set; } = string.Empty;
    }
}
