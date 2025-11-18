using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BackendEcommerce.Application.Features.SellerRegistration.Contracts;
using BackendEcommerce.Application.Features.SellerRegistration.DTOs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BackendEcommerce.Application.Features.SellerRegistration
{
    [ApiController]
    [Route("api/seller")]
    public class SellerRegistrationController : ControllerBase
    {
        private readonly ISellerRegistrationService _registrationService;
        private readonly ILogger<SellerRegistrationController> _logger;

        public SellerRegistrationController(
            ISellerRegistrationService registrationService,
            ILogger<SellerRegistrationController> logger)
        {
            _registrationService = registrationService;
            _logger = logger;
        }

        [HttpPost("registration")]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> RegisterSeller([FromBody] SellerRegistrationDTOs dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var shopId = await _registrationService.RegisterSellerAsync(dto, User);
                return Created($"/api/shops/{shopId}", new 
                { 
                    message = "Đăng ký shop thành công. Bạn đã được nâng cấp lên 'seller'.", 
                    shopId 
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while registering seller.");
                return StatusCode(500, new { message = "Lỗi hệ thống.", details = ex.Message });
            }
        }
    }
}
