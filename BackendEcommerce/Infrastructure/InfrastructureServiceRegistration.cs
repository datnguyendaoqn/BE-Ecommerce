using BackendEcommerce.Domain.Contracts.Caching;
using BackendEcommerce.Domain.Contracts.Email;
using BackendEcommerce.Domain.Contracts.Persistence;
using BackendEcommerce.Domain.Contracts.Services;
using BackendEcommerce.Infrastructure.Caching.Redis;
using BackendEcommerce.Infrastructure.Email;
using BackendEcommerce.Infrastructure.Medias;
using BackendEcommerce.Infrastructure.Persistence.Data;
using BackendEcommerce.Infrastructure.Persistence.Repositories;
using BackendEcommerce.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Resend;
using StackExchange.Redis;
using System.Text;

namespace BackendEcommerce.Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // 1. Cấu hình Persistence (Oracle DB)
            // Lấy biến môi trường và xây dựng connection string
            var user = configuration["ORACLE_USER"];
            var password = configuration["ORACLE_PASSWORD"];
            var service = configuration["ORACLE_SERVICE"];
            var host = configuration["ORACLE_HOST"];
            var port = configuration["ORACLE_PORT"];
            var connectionString = $"User Id={user};Password={password};Data Source={host}:{port}/{service}";

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
                throw new Exception("Missing Oracle credentials in environment variables.");

            services.AddDbContext<EcomDbContext>(options =>
                options.UseOracle(connectionString)
            );

            // Đăng ký Repositories
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IShopRepository, ShopRepository>();
            services.AddScoped<IMediaRepository, MediaRepository>();
            services.AddScoped<IMediaUploadService, CloudinaryMediaService>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();

            // (Và cấu hình Cloudinary Account từ IConfiguration...)

            // 2. Cấu hình Caching (Redis)
            var redisConnectionString = configuration["REDIS_CONNECTION"];
            if (string.IsNullOrEmpty(redisConnectionString))
                throw new Exception("Missing Redis connection string in environment variables.");

            services.AddSingleton<IConnectionMultiplexer>(sp =>
                ConnectionMultiplexer.Connect(redisConnectionString)
            );
            services.AddScoped<IOtpRepository, RedisOtpRepository>();

            // 3. Cấu hình Email (Resend)
            var resendApiKey = configuration["RESEND_APIKEY"];
            if (string.IsNullOrEmpty(resendApiKey))
                throw new Exception("Missing Resend API key in environment variables.");

            services.AddSingleton<IResend>(_ => ResendClient.Create(resendApiKey));
            services.AddScoped<IEmailService, ResendEmailService>();

            // 4. Cấu hình Security (JwtHelper)
            var jwtKey = configuration["JWT_KEY"];
            if (string.IsNullOrEmpty(jwtKey))
                throw new Exception("Missing JWT_KEY in environment variables.");

            services.AddSingleton(new JwtHelper(jwtKey));

            //5. JWT
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // This is the logic from your JwtHelper!
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true, // Checks if expired
                    ValidateIssuerSigningKey = true, // Checks the signature
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT_KEY"]))
                };
            });
            return services;

        }
    }
}
