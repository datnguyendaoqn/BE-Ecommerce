using Microsoft.EntityFrameworkCore;
using BackendEcommerce.Application.Features.Admin.Contracts;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using BackendEcommerce.Infrastructure.Persistence.Data;

namespace BackendEcommerce.Application.Features.Admin
{
    public class UpdateUserRoleService : IUpdateUserRoleService
    {
        private readonly EcomDbContext _context;

        public UpdateUserRoleService(EcomDbContext context)
        {
            _context = context;
        }

        public async Task UpdateUserRoleAsync(long userId, string newRole)
        {
            // 1. Chuẩn hóa và Kiểm tra vai trò
            // (Dựa trên schema CSDL của bạn, vai trò mặc định là "customer")
            var normalizedRole = newRole.ToLowerInvariant();
            if (normalizedRole != "customer" && normalizedRole != "seller")
            {
                // Ném lỗi để Controller bắt và trả về 400 Bad Request
                throw new ArgumentException("Vai trò không hợp lệ. Chỉ chấp nhận 'customer' hoặc 'seller'.");
            }

            // 2. Tìm User
            // Giả sử bạn có một Entity 'User' ánh xạ với bảng USERS
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                // Ném lỗi để Controller bắt và trả về 404 Not Found
                throw new KeyNotFoundException($"Không tìm thấy User với ID {userId}.");
            }

            // 3. Cập nhật và Lưu
            user.Role = normalizedRole;
            user.UpdatedAt = DateTime.UtcNow; // Đừng quên cập nhật thời gian

            await _context.SaveChangesAsync();
        }
    }
}