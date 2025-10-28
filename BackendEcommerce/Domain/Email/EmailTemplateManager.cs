namespace BackendEcommerce.Domain.Email
{
    public class EmailTemplateManager : IEmailTemplateManager
    {
        private readonly string _templatePath;
        private readonly IWebHostEnvironment _env;

        public EmailTemplateManager(IWebHostEnvironment env)
        {
            // path in folder bin -  build to have folder , register csproj ItemGroup
            _env = env;
            _templatePath = Path.Combine(_env.ContentRootPath,"Infrastructure", "Email", "Templates", "OtpEmailTemplate.html");
        }

        public string GetOtpTemplate()
        {
            if (!File.Exists(_templatePath))
                throw new FileNotFoundException("OTP email template not found.", _templatePath);

            return File.ReadAllText(_templatePath);
        }
    }
}
