using BackendEcommerce.Application.Features.Auth;
using BackendEcommerce.Infrastructure.Persistence.Models;
using System.Security.Cryptography;

namespace BackendEcommerce.Domain.Auth
{
    public class TokenManager
    {
        private readonly JwtHelper _jwtHelper;

        public TokenManager(JwtHelper jwtHelper)
        {
            _jwtHelper = jwtHelper;
        }

        public string GenerateAccessToken(int accountId, string username, string role)
        {
            return _jwtHelper.GenerateToken(accountId, username, role);
        }

        public RefreshToken GenerateRefreshToken(string? ipAddress)
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            return new RefreshToken
            {
                Token = token,
                Expires = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };
        }
    }
}
