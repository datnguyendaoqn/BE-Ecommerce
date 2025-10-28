using System.Security.Cryptography;

namespace BackendEcommerce.Domain.Otp
{
    public static class OtpHelper
    {
        public static string GenerateSecureOtp(int length = 6)
        {
            var bytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            // Chuyển mỗi byte thành 0-9
            var otpChars = bytes.Select(b => (b % 10).ToString()).ToArray();
            return string.Join("", otpChars);
        }
    }
}
