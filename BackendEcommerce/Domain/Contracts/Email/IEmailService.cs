using BackendEcommerce.Domain.Email;

namespace BackendEcommerce.Domain.Contracts.Email
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(EmailMessageDTO obj);
    }
}
