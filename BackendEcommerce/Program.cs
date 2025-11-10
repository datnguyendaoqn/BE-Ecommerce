using BackendEcommerce.Application;
using BackendEcommerce.Domain;
using BackendEcommerce.Infrastructure;
using BackendEcommerce.Infrastructure.Persistence.Data;
using DotNetEnv;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Tải file .env VÀO IConfiguration
// Phải làm điều này ĐẦU TIÊN
Env.Load();
builder.Configuration.AddEnvironmentVariables();

// 2. Đăng ký dịch vụ từ các Layer
builder.Services
    .AddApplicationServices(builder.Configuration)
    .AddDomainServices()
    .AddInfrastructureServices(builder.Configuration); // Truyền IConfiguration vào

// 3. Đăng ký các dịch vụ của Framework (Controllers, Swagger, CORS)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
//Thêm Bearer cho Swagger 
builder.Services.AddSwaggerGen(options =>
{
    // 1. Thêm định nghĩa bảo mật (Security Definition)
    // Chúng ta định nghĩa một "khóa" tên là "Bearer"
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Nhập JWT token của bạn vào đây. \n\nVí dụ: '12345abcde' (Swagger sẽ tự thêm 'Bearer ')",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http, 
        Scheme = "Bearer", // <-- Tên của lược đồ
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer" // Phải khớp với tên ở AddSecurityDefinition
                }
            },
            new string[] {}
        }
    });
});

// --- Xây dựng App ---
var app = builder.Build();

// --- Các tác vụ khởi động (Ví dụ: DB check) ---
// (Bạn có thể bỏ logic này vào một hàm private hoặc file khác nếu muốn)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EcomDbContext>();
    try
    {
        if (db.Database.CanConnect())
        {
            Console.WriteLine("✅ Connected to Oracle DB successfully!");
        }
        else
        {
            Console.WriteLine("❌ Cannot connect to Oracle DB.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Connection failed: {ex.Message}");
    }
}
//

// --- Cấu hình HTTP Request Pipeline ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();