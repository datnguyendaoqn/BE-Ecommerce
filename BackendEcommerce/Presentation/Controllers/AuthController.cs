using BackendEcommerce.Application.Features.Auth.Contracts;
using BackendEcommerce.Application.Features.Auth.DTOs;
using BackendEcommerce.Application.Shared.DTOs;
using BackendEcommerce.Infrastructure.Persistence.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resend;
using System.Net.Mail;
using System.Security.Claims;

namespace BackendEcommerce.Presentation.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly EcomDbContext _context;

        public AuthController(IAuthService authService, EcomDbContext context)
        {
            _context = context;
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO dto)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var result = await _authService.LoginAsync(dto, ip);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpPost("register-otp")]
        public async Task<IActionResult> RequestOtp([FromBody] RequestRegisterOtpDTO dto)
        {
            var result = await _authService.RequestRegisterOtpAsync(dto);
            return result.IsSuccess ? Ok(result):BadRequest(result);
        }
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponseDTO<LoginResponseDTO>>> Register(RegisterRequestDTO dto)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var response = await _authService.RegisterAsync(dto, ipAddress);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpPost("login-with-otp")]
        public async Task<IActionResult> LoginWithOtp([FromBody] LoginWithOtpDTO dto)
        {
            var result = await _authService.LoginWithOtpAsync(dto, dto.Otp);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("me")]
        [Authorize] // BẮT BUỘC Đăng nhập
        public async Task<ActionResult<ApiResponseDTO<AuthMeResponseDto>>> GetMe()
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (customerId == null)
            {
                return Unauthorized(new ApiResponseDTO<string>
                {
                    IsSuccess = false,
                    Code = 401,
                    Message = "Unauthorized"
                });
            }
                var response = await _authService.GetMeAsync(int.Parse(customerId));

            return Ok(response);
        }
    }
}
