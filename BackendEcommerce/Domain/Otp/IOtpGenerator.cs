using BackendEcommerce.Application.Auth.DTOs;

namespace BackendEcommerce.Domain.Otp
{
    public interface IOtpGenerator
    {
        Task GenerateAndSendAsync(string? email);
    }
}
    