using BackendEcommerce.Application.Shared.Contracts;
using BackendEcommerce.Domain.Email;
using Resend;

namespace BackendEcommerce.Infrastructure.Email
{
    public class ResendEmailService : IEmailService
    {
        private readonly IResend _client;
       

        public ResendEmailService(IResend client)
        {
            _client = client;
        }

        public async Task SendOtpEmailAsync(EmailMessageDTO dto)
        {          
            var response = await _client.EmailSendAsync(new EmailMessage
            {
                From = dto.From,
                To = dto.To,
                Subject = dto.Subject,
                HtmlBody = dto.HtmlBody
            });

            if (!response.Success)
                throw new Exception($"Email failed: {response.Exception}");
        }
    }
}
