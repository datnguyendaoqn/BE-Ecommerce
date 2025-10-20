using DotNetEnv;
using ECommerceApp.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
// load Env
Env.Load();
// get env variables
var user = Env.GetString("ORACLE_USER");
var password = Env.GetString("ORACLE_PASSWORD");
var pdb = Env.GetString("ORACLE_PDB");
var host = Env.GetString("ORACLE_HOST");
var port = Env.GetString("ORACLE_PORT");
var service = Env.GetString("ORACLE_SERVICE");
// Build connection string
var connectionString = $"User Id={user};Password={password};Data Source={host}:{port}/{service}";

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
// Add DbContext
builder.Services.AddDbContext<EcomDbContext>(options =>
    options.UseOracle(connectionString)
);
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<EcomDbContext>();

try
{
    // Kiểm tra kết nối
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
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();

app.Run();
