using BackendEcommerce.Infrastructure.Persistence.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace BackendEcommerce.Presentation.Controllers
{
    [Route("api/health")]
    [ApiController]
    public class HealthCheckController : ControllerBase
    {
        private readonly EcomDbContext _dbContext;
        private readonly IConnectionMultiplexer _redis;

        public HealthCheckController(
            EcomDbContext dbContext,
            IConnectionMultiplexer redis)
        {
            _dbContext = dbContext;
            _redis = redis;
        }

        [HttpGet]
        public async Task<IActionResult> CheckHealth()
        {
            var dbStatus = "Checking...";
            var redisStatus = "Checking...";

      

            // --- 2. Test kết nối Redis (SET/GET) ---
            try
            {
                // Lấy kết nối DB của Redis
                var redisDb = _redis.GetDatabase();

                // Thử GHI (SET) một key
                bool setSuccess = await redisDb.StringSetAsync("health_check_key", "OK", TimeSpan.FromSeconds(10));

                // Thử ĐỌC (GET) lại key đó
                string? getValue = await redisDb.StringGetAsync("health_check_key");

                if (setSuccess && getValue == "OK")
                {
                    redisStatus = "OK (SET/GET Success)";
                }
                else
                {
                    redisStatus = "FAILED (Could not SET/GET key)";
                }
            }
            catch (Exception ex)
            {
                redisStatus = $"FAILED: {ex.Message}";
            }

            // --- 3. Trả về kết quả ---
            return Ok(new
            {
                Status = "Health Check Complete",
                DatabaseStatus = dbStatus,
                RedisStatus = redisStatus
            });
        }
    }
}
