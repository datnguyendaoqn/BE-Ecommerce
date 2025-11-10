using BackendEcommerce.Domain.Email;

namespace BackendEcommerce.Application.Shared.Contracts
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(EmailMessageDTO obj);
    }
}
