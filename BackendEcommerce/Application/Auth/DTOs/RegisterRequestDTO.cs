using System.ComponentModel.DataAnnotations;

namespace BackendEcommerce.Application.Auth.DTOs
{
    public class RegisterRequestDTO
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = null!;

        public string? FullName { get; set; }

        [Required(ErrorMessage = "OTP is required")]
        [Length(6, 6, ErrorMessage = "OTP must be 6 characters")]
        public string Otp { get; set; } = null!;
    }
}
