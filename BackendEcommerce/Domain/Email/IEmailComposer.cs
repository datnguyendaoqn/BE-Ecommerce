namespace BackendEcommerce.Domain.Email
{
    public interface IEmailComposer
    {
        EmailMessageDTO ComposeOtpEmail(string toEmail, string otp);
    }
}
