using BackendEcommerce.Application.Auth.DTOs;
using BackendEcommerce.Application.Shared.DTOs;
using BackendEcommerce.Domain.Auth;
using BackendEcommerce.Domain.Contracts.Persistence;
using BackendEcommerce.Domain.Otp;
using BackendEcommerce.Infrastructure.Persistence.Models;
namespace BackendEcommerce.Application.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IAccountRepository _accountRepo;
        private readonly UserAuthenticator _userAuth;
        private readonly TokenManager _tokenManager;
        private readonly IOtpGenerator _otpGenerator;
        private readonly IOtpValidator _otpValidator;
        private readonly PasswordHasher _passwordHasher;

        public AuthService(
             IAccountRepository accountRepo,
             UserAuthenticator userAuthenticator,
             TokenManager tokenManager,
             IOtpGenerator otpGenerator,
             IOtpValidator otpValidator,
             PasswordHasher passwordHasher)
        {
            _accountRepo = accountRepo;
            _userAuth = userAuthenticator;
            _tokenManager = tokenManager;
            _otpGenerator = otpGenerator;
            _otpValidator = otpValidator;
            _passwordHasher = passwordHasher;
        }

        public async Task<ApiResponseDTO<LoginResponseDTO?>> LoginAsync(LoginRequestDTO dto, string? ipAddress)
        {
            var account = await _accountRepo.GetByEmailAsync(dto.Email);

            if (!_userAuth.ValidateCredentials(account, dto.Password))
            {
                return new ApiResponseDTO<LoginResponseDTO?>
                {
                    IsSuccess = false,
                    Code = 401,
                    Message = "Email hoặc password không đúng",
                    Data = null
                };
            }

            var accessToken = _tokenManager.GenerateAccessToken(account!.Id, account.Username, account.User.Role);
            var refreshToken = _tokenManager.GenerateRefreshToken(ipAddress);

            account.RefreshTokens.Add(refreshToken);
            await _accountRepo.SaveChangesAsync();

            return new ApiResponseDTO<LoginResponseDTO?>
            {
                IsSuccess = true,
                Code = 200,
                Message = "Login successfully",
                Data = new LoginResponseDTO
                {
                    ID = account.Id.ToString(),
                    FullName= account.User.FullName,
                    Role= account.User.Role,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken.Token
                }
            };
        }
        public async Task<ApiResponseDTO<string>> RequestLoginOtpAsync(RequestLoginDTO obj)
        {
            return await SendOtpToExistingUserAsync(obj.Email, "Email không đúng.");
        }
        public async Task<ApiResponseDTO<string>> RequestRegisterOtpAsync(RequestRegisterOtpDTO dto)
        {
            // 1. Application Rule: Check user (Email) KHÔNG TỒN TẠI
            var accountWithEmail = await _accountRepo.GetByEmailAsync(dto.Email);
            if (accountWithEmail != null)
            {
                return new ApiResponseDTO<string>
                {
                    IsSuccess = false,
                    Code = 400,
                    Message = "Email is already existed."
                };
            }

            // 2. Gọi (Domain) để gửi OTP
            await _otpGenerator.GenerateAndSendAsync(dto.Email);
            return new ApiResponseDTO<string> { IsSuccess = true, Message = "OTP is sended" };
        }
        public async Task<ApiResponseDTO<LoginResponseDTO>> RegisterAsync(RegisterRequestDTO dto, string? ipAddress)
        {
            // 1. Gọi "Não bộ" (Domain) để check OTP
            var isValidOtp = await _otpValidator.VerifyAsync(dto.Email, dto.Otp);
            if (!isValidOtp)
            {
                return new ApiResponseDTO<LoginResponseDTO>
                {
                    IsSuccess = false,
                    Code = 400,
                    Message = "Invalid or expired OTP."
                };
            }

            // 2. Application Rule: Check lại (phòng hờ race condition)
            if (await _accountRepo.GetByEmailAsync(dto.Email) != null ||
                await _accountRepo.GetByUsernameAsync(dto.Username) != null)
            {
                return new ApiResponseDTO<LoginResponseDTO>
                {
                    IsSuccess = false,
                    Code = 400,
                    Message = "Email or Username is already existed."
                };
            }

            var passwordHash = _passwordHasher.Hash(dto.Password);

            // 4. Tạo các thực thể (Entities) theo đúng Model của bạn
            var newUser = new User
            {
                Email = dto.Email,
                FullName = dto.FullName,
                Role = "customer", // Model của bạn đã có default, nhưng set ở đây rõ ràng hơn
                CreatedAt = DateTime.UtcNow
            };

            var newAccount = new Account
            {
                Username = dto.Username,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow,
                User = newUser // Gắn User vào Account. EF Core sẽ tự hiểu
            };

            // 5. Gọi "Tay chân" (Infra) để lưu
            await _accountRepo.AddAsync(newAccount);
            await _accountRepo.SaveChangesAsync();

            // 6. Tự động đăng nhập: Gọi "Não bộ" (Domain) tạo Token
            var accessToken = _tokenManager.GenerateAccessToken(newAccount.Id, newAccount.Username, newUser.Role);

            // Giả định: _tokenManager.GenerateRefreshToken trả về 1 entity RefreshToken
            var refreshToken = _tokenManager.GenerateRefreshToken(ipAddress);

            newAccount.RefreshTokens.Add(refreshToken);
            await _accountRepo.SaveChangesAsync(); // Lưu RefreshToken

            // 7. Trả về token (đăng nhập thành công)
            return new ApiResponseDTO<LoginResponseDTO>
            {
                IsSuccess = true,
                Message = "Registration successful",
                Data = new LoginResponseDTO
                {
                    AccessToken = accessToken,
                    // Giả định: entity RefreshToken có 1 property tên .Token (string)
                    RefreshToken = refreshToken.Token
                }
            };
        }
        private async Task<ApiResponseDTO<string>> SendOtpToExistingUserAsync(string? email, string notFoundMessage)
        {
            // Logic 2: Check user PHẢI TỒN TẠI
            var account = await _accountRepo.GetByEmailAsync(email);
            if (account == null)
            {
                return new ApiResponseDTO<string>
                {
                    IsSuccess = false,
                    Code = 404,
                    Message = notFoundMessage // Dùng message lỗi phù hợp
                };
            }

            // Gọi "chuyên gia" Domain
            await _otpGenerator.GenerateAndSendAsync(email);

            return new ApiResponseDTO<string> { IsSuccess = true, Message = "OTP is sended" };
        }
        // === THÊM PHƯƠNG THỨC MỚI: Đăng nhập bằng OTP ===
        public async Task<ApiResponseDTO<LoginResponseDTO?>> LoginWithOtpAsync(LoginWithOtpDTO dto, string? ipAddress)
        {
            // 1. Điều phối: Gọi "chuyên gia" Domain để xác thực OTP
            var isValid = await _otpValidator.VerifyAsync(dto.Email, dto.Otp);
            if (!isValid)
            {
                return new ApiResponseDTO<LoginResponseDTO?> { IsSuccess = false, Message = "Otp is invalid" };
            }

            // 2. Điều phối: Lấy thông tin user
            var account = await _accountRepo.GetByEmailAsync(dto.Email);

            // 3. Điều phối: Gọi "chuyên gia" Domain để tạo token (giống hệt LoginAsync)
            var accessToken = _tokenManager.GenerateAccessToken(account!.Id, account.Username, account.User.Role);
            var refreshToken = _tokenManager.GenerateRefreshToken(ipAddress);

            account.RefreshTokens.Add(refreshToken);
            await _accountRepo.SaveChangesAsync();

            // 4. Trả về DTO
            return new ApiResponseDTO<LoginResponseDTO?>
            {
                IsSuccess = true,
                Code = 200,
                Message = "Login successfully",
                Data = new LoginResponseDTO
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken.Token
                }
            };
        }
    }
}
