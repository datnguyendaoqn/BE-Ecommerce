using Microsoft.Extensions.DependencyInjection;
// Nhớ using các thư mục con của Application
using BackendEcommerce.Application.Auth;

namespace BackendEcommerce.Application
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Đăng ký tất cả "Quản lý" (Use Case) ở đây

            // Từ /Application/Auth
            services.AddScoped<IAuthService, AuthService>();

            // Từ /Application/Products (Khi nào bạn tạo)
            // services.AddScoped<IProductService, ProductService>();

            return services;
        }
    }
}