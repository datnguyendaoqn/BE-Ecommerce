namespace BackendEcommerce.Application.Auth.DTOs
{
    public class LoginResponseDTO
    {
        public string FullName { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string ID { get; set; } = null!;
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;

    }

}
    