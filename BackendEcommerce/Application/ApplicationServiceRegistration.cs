// Nhớ using các thư mục con của Application
using BackendEcommerce.Application.Auth;
using BackendEcommerce.Application.Categories;
using BackendEcommerce.Application.Medias;
using BackendEcommerce.Application.Products;
using BackendEcommerce.Application.Reviews;

namespace BackendEcommerce.Application
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Đăng ký tất cả "Quản lý" (Use Case) ở đây
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IMediaService, MediaService>();
            return services;
        }
    }
}