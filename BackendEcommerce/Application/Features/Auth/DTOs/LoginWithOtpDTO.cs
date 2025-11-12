namespace BackendEcommerce.Application.Features.Auth.DTOs
{
    public class LoginWithOtpDTO
    {
        public string? Email { get; set; }
        public string? Otp { get; set; }
    }
}
