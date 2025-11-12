using System.ComponentModel.DataAnnotations;

namespace BackendEcommerce.Application.Features.Admin.DTOs
{
    public class UpdateUserRoleDTOs
    {
        [Required(ErrorMessage = "Role is required")]
        [RegularExpression("^(admin|seller|customer)$", ErrorMessage = "Role must be either 'admin', 'seller', or 'customer'")]
        public string NewRole { get; set; } = null!;
    }
}