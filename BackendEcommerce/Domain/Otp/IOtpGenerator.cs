using BackendEcommerce.Application.Features.Auth.DTOs;

namespace BackendEcommerce.Domain.Otp
{
    public interface IOtpGenerator
    {
        Task GenerateAndSendAsync(string? email);
    }
}
    