using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace BackendEcommerce.Application.Features.Auth
{
    public class JwtHelper
    {
        private readonly string _secretKey;

        public JwtHelper(string secretKey)
        {
            _secretKey = secretKey;
        }

        // 1️⃣ Generate JWT token
        public string GenerateToken(int accountId, string email, string role, int expireMinutes = 60)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, accountId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role),
            };

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expireMinutes),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            };

            var handler = new JsonWebTokenHandler();
            return handler.CreateToken(descriptor);
        }

        // 2️⃣ Validate token
        public async Task<ClaimsIdentity?> ValidateTokenAsync(string token)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            };

            var handler = new JsonWebTokenHandler();
            var result = await handler.ValidateTokenAsync(token, validationParameters);

            if (!result.IsValid)
            {
                Console.WriteLine($"JWT invalid: {result.Exception?.Message}");
                return null;
            }

            return result.ClaimsIdentity;
        }

        // 3️⃣ Decode token (read payload only)
        public void DecodeToken(string token)
        {
            var handler = new JsonWebTokenHandler();
            var jwt = handler.ReadJsonWebToken(token);
            var alg = jwt.GetHeaderValue<string>("alg");
            var typ = jwt.GetHeaderValue<string>("typ");

            Console.WriteLine($"alg: {alg}");
            Console.WriteLine($"typ: {typ}");
        }
    }
}
