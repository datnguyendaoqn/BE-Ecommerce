using BackendEcommerce.Application;
using BackendEcommerce.Domain;
using BackendEcommerce.Infrastructure;
using BackendEcommerce.Infrastructure.Persistence.Data;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// 1. Tải file .env VÀO IConfiguration
// Phải làm điều này ĐẦU TIÊN
Env.Load();
builder.Configuration.AddEnvironmentVariables();

// 2. Đăng ký dịch vụ từ các Layer
builder.Services
    .AddApplicationServices()
    .AddDomainServices()
    .AddInfrastructureServices(builder.Configuration); // Truyền IConfiguration vào

// 3. Đăng ký các dịch vụ của Framework (Controllers, Swagger, CORS)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
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

// --- Cấu hình HTTP Request Pipeline ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization(); // (Lưu ý: Bạn nên thêm UseAuthentication() nếu dùng JWT)
app.MapControllers();

app.Run();