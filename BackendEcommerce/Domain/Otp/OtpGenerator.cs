using BackendEcommerce.Application.Auth.DTOs;
using BackendEcommerce.Domain.Contracts.Caching;
using BackendEcommerce.Domain.Contracts.Email;
using BackendEcommerce.Domain.Email;

namespace BackendEcommerce.Domain.Otp
{
    public class OtpGenerator : IOtpGenerator
    {
        private readonly IOtpRepository _otpRepository;
        private readonly IEmailComposer _emailComposer; // <-- Dùng Domain Model
        private readonly IEmailService _emailService;     // <-- Dùng "Tay chân"

        public OtpGenerator(IOtpRepository otpRepository,
                            IEmailComposer emailComposer,
                            IEmailService emailService)
        {
            _otpRepository = otpRepository;
            _emailComposer = emailComposer;
            _emailService = emailService;
        }

        public async Task GenerateAndSendAsync(string email)
        {
            // 1. Quy tắc nghiệp vụ: Tạo OTP
            var otp = OtpHelper.GenerateSecureOtp();

            // 2. Quy tắc nghiệp vụ: Hạn 2 phút
            await _otpRepository.SaveOtpAsync(email, otp, TimeSpan.FromMinutes(2));

            // 3. Quy tắc nghiệp vụ: Soạn email
            var emailMessage = _emailComposer.ComposeOtpEmail(email, otp);

            // 4. Yêu cầu "tay chân" gửi
            await _emailService.SendOtpEmailAsync(emailMessage);
        }
    }
}
