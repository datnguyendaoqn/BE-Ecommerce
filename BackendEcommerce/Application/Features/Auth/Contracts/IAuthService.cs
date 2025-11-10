using BackendEcommerce.Application.Features.Auth.DTOs;
using BackendEcommerce.Application.Shared.DTOs;

namespace BackendEcommerce.Application.Features.Auth.Contracts
{
    public interface IAuthService
    {
        Task<ApiResponseDTO<LoginResponseDTO?>> LoginAsync(LoginRequestDTO dto, string ipAddress);
        Task<ApiResponseDTO<string>> RequestLoginOtpAsync(RequestLoginDTO obj);
        Task<ApiResponseDTO<LoginResponseDTO?>> LoginWithOtpAsync(LoginWithOtpDTO dto, string? ipAddress);
        Task<ApiResponseDTO<string>> RequestRegisterOtpAsync(RequestRegisterOtpDTO dto);
        Task<ApiResponseDTO<LoginResponseDTO>> RegisterAsync(RegisterRequestDTO dto, string? ipAddress);
    }
}
