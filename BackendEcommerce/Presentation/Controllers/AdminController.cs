using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BackendEcommerce.Application.Features.Admin.Contracts; // Service Interface
using BackendEcommerce.Application.Features.Admin.DTOs;     // DTO
using System;
using System.Collections.Generic; // Cần cho KeyNotFoundException
using System.Threading.Tasks;

namespace BackendEcommerce.Presentation.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "admin")] // Chỉ admin mới có quyền truy cập
    public class AdminController : ControllerBase
    {
        private readonly IUpdateUserRoleService _updateUserRoleService;

        public AdminController(IUpdateUserRoleService updateUserRoleService)
        {
            _updateUserRoleService = updateUserRoleService;
        }

            [HttpPut("users/{userId}/role")]
        public async Task<IActionResult> UpdateUserRole(long userId, [FromBody] UpdateUserRoleDTOs dto)
        {
            try
            {
                await _updateUserRoleService.UpdateUserRoleAsync(userId, dto.NewRole);
                return NoContent(); // Trả về 204 No Content khi cập nhật thành công
            }
            catch (ArgumentException ex) // Bắt lỗi nếu vai trò không hợp lệ
            {
                return BadRequest(new { message = ex.Message }); // Trả về 400
            }
            catch (KeyNotFoundException ex) // Bắt lỗi nếu không tìm thấy user
            {
                return NotFound(new { message = ex.Message }); // Trả về 404
            }
            catch (Exception ex)
            {
                // Bắt các lỗi chung khác
                return StatusCode(500, new { message = "Lỗi hệ thống.", details = ex.Message });
            }
        }
    }
}