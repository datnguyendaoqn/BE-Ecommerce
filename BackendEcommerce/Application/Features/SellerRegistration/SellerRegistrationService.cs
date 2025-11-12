
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BackendEcommerce.Application.Features.SellerRegistration.DTOs;
using BackendEcommerce.Infrastructure.Persistence.Data;
using BackendEcommerce.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using BackendEcommerce.Application.Features.SellerRegistration.Contracts;

namespace BackendEcommerce.Application.Features.SellerRegistration
{
    public class SellerRegistrationService : ISellerRegistrationService
    {
        private readonly EcomDbContext _context;

        public SellerRegistrationService(EcomDbContext context)
        {
            _context = context;
        }


        public async Task<int> RegisterSellerAsync(SellerRegistrationDTOs dto, ClaimsPrincipal User)
        {
            // 1. Lấy User ID từ Claims (token)
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                // Lỗi này không nên xảy ra nếu [Authorize] hoạt động đúng
                throw new UnauthorizedAccessException("Không thể xác định ID người dùng.");
            }

            // 2. Lấy thông tin User từ CSDL
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Người dùng không tồn tại.");
            }

            // 3. Kiểm tra các điều kiện nghiệp vụ
            if (user.Role != "customer")
            {
                throw new InvalidOperationException("Chỉ có 'customer' mới có thể đăng ký bán hàng.");
            }

            var existingShop = await _context.Shops.FirstOrDefaultAsync(s => s.OwnerId == userId);
            if (existingShop != null)
            {
                throw new InvalidOperationException("Bạn đã đăng ký một shop rồi.");
            }

            // 4. Tạo Shop mới
            var newShop = new Shop // Giả sử bạn có class Entity 'Shop'
            {
                OwnerId = userId,
                Name = dto.Name,
                Description = dto.Description,
                BankAccountNumber = dto.BankAccountNumber,
                Status = "active", // Trạng thái của shop
                CccdStatus = "PENDING", // Trạng thái CCCD (chờ admin duyệt)
                BankStatus = "PENDING", // Trạng thái Bank (chờ admin duyệt)
                CreatedAt = DateTime.UtcNow
            };

            await _context.Shops.AddAsync(newShop);

            // 5. Nâng cấp vai trò (role) của User
            user.Role = "seller";
            user.UpdatedAt = DateTime.UtcNow;

            // 6. Lưu tất cả thay đổi
            await _context.SaveChangesAsync();

            return newShop.Id;
        }
    }
}