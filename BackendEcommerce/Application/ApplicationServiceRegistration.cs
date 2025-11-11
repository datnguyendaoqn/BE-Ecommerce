// Nhớ using các thư mục con của Application
using BackendEcommerce.Application.Features.Auth;
using BackendEcommerce.Application.Features.Auth.Contracts;
using BackendEcommerce.Application.Features.Carts;
using BackendEcommerce.Application.Features.Carts.Contracts;
using BackendEcommerce.Application.Features.Categories;
using BackendEcommerce.Application.Features.Categories.Contracts;
using BackendEcommerce.Application.Features.Medias;
using BackendEcommerce.Application.Features.Medias.Contracts;
using BackendEcommerce.Application.Features.Products;
using BackendEcommerce.Application.Features.Products.Contracts;
using BackendEcommerce.Application.Features.Reviews;
using BackendEcommerce.Application.Features.Reviews.Contracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BackendEcommerce.Application
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Đăng ký tất cả "Quản lý" (Use Case) ở đây
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IMediaService, MediaService>();
            services.AddScoped<ICartService, CartService>();
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