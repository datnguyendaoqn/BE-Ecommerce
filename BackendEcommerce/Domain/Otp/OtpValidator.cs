using BackendEcommerce.Application.Shared.Contracts;

namespace BackendEcommerce.Domain.Otp
{
    public class OtpValidator : IOtpValidator
    {
        private readonly IOtpRepository _otpRepository;

        public OtpValidator(IOtpRepository otpRepository)
        {
            _otpRepository = otpRepository;
        }

        public async Task<bool> VerifyAsync(string email, string otp)
        {
            // 1. Quy tắc nghiệp vụ: Lấy OTP
            var storedOtp = await _otpRepository.GetOtpAsync(email);

            // 2. Quy tắc nghiệp vụ: So sánh
            if (storedOtp == null || storedOtp != otp)
                return false;

            // 3. Quy tắc nghiệp vụ: Xóa sau khi dùng
            await _otpRepository.DeleteOtpAsync(email);
            return true;
        }
    }
}
