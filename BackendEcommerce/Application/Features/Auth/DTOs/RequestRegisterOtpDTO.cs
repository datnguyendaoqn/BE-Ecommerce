using System.ComponentModel.DataAnnotations;

namespace BackendEcommerce.Application.Features.Auth.DTOs
{
    public class RequestRegisterOtpDTO
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = null!;
    }
}
