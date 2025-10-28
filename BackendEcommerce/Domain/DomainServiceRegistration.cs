// Nhớ using các thư mục con của Domain
using BackendEcommerce.Domain.Auth;
using BackendEcommerce.Domain.Email;
using BackendEcommerce.Domain.Otp;

namespace BackendEcommerce.Domain
{
    public static class DomainServiceRegistration
    {
        public static IServiceCollection AddDomainServices(this IServiceCollection services)
        {
            // Đăng ký tất cả "Não bộ" ở đây

            // Từ /Domain/Auth
            services.AddScoped<PasswordHasher>();
            services.AddScoped<TokenManager>();
            services.AddScoped<UserAuthenticator>();

            // Từ /Domain/Email
            services.AddScoped<IEmailComposer, EmailComposer>();
            services.AddScoped<IEmailTemplateManager, EmailTemplateManager>();

            // Từ /Domain/Otp
            services.AddScoped<IOtpGenerator, OtpGenerator>();
            services.AddScoped<IOtpValidator, OtpValidator>();
            // OtpHelper có thể là static, không cần DI

            return services;
        }
    }
}