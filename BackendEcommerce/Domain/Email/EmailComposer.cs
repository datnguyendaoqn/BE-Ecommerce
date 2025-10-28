namespace BackendEcommerce.Domain.Email
{
    public class EmailComposer : IEmailComposer
    {
        private readonly IEmailTemplateManager _templateManager;

        public EmailComposer(IEmailTemplateManager templateManager)
        {
            _templateManager = templateManager;
        }

        public EmailMessageDTO ComposeOtpEmail(string toEmail, string otp)
        {
            var html = _templateManager.GetOtpTemplate();
            html = html.Replace("{{OTP_CODE}}", otp);

            return new EmailMessageDTO
            {
                To = toEmail,
                Subject = "OTP Verification", // Quy tắc nghiệp vụ: Subject là đây
                HtmlBody = html
            };
        }
    }
}
