using BackendEcommerce.Application.Auth;
using BackendEcommerce.Application.Auth.DTOs;
using BackendEcommerce.Application.Shared.DTOs;
using BackendEcommerce.Infrastructure.Persistence.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resend;
using System.Net.Mail;

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
    }
}
