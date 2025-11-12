using BackendEcommerce.Application.Shared.Contracts;
using StackExchange.Redis;

namespace BackendEcommerce.Infrastructure.Caching.Redis
{
    public class RedisOtpRepository : IOtpRepository
    {
        private readonly IDatabase _db;

        public RedisOtpRepository(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public async Task SaveOtpAsync(string email, string otp, TimeSpan expiry)
        {
            await _db.StringSetAsync(email, otp, expiry);
        }

        public async Task<string?> GetOtpAsync(string email)
        {
            return await _db.StringGetAsync(email);
        }

        public async Task DeleteOtpAsync(string email)
        {
            await _db.KeyDeleteAsync(email);
        }
    }
}
