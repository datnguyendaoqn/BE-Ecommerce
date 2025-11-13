using BackendEcommerce.Application.Features.Orders.Contracts;
using BackendEcommerce.Infrastructure.Persistence.Data;
using BackendEcommerce.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendEcommerce.Infrastructure.Persistence.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly EcomDbContext _context;

        public OrderRepository(EcomDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        /// <summary>
        /// Triển khai logic lấy đơn hàng cho Seller
        /// </summary>
        public async Task<(IEnumerable<Order> Orders, int TotalCount)> GetOrdersByShopIdAsync(
            int shopId,
            string? status,
            int pageNumber,
            int pageSize)
        {
            // 1. Xây dựng query cơ sở
            // BAO GỒM OrderItems để Service map sang DTO
            var query = _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.ShopId == shopId)
                .AsNoTracking(); // Tăng hiệu năng vì đây là query Read-Only

            // 2. Áp dụng Filter (nếu có)
            if (!string.IsNullOrEmpty(status))
            {
                // So sánh không phân biệt hoa thường
                var statusLower = status.ToLower();
                query = query.Where(o => o.Status.ToLower() == statusLower);
            }

            // 3. Lấy tổng số lượng (trước khi phân trang)
            // Cần thực thi CountAsync trên query ĐÃ filter
            var totalCount = await query.CountAsync();

            // 4. Áp dụng Sắp xếp và Phân trang
            // Sử dụng pageNumber (từ 1) thay vì pageIndex (từ 0)
            var orders = await query
                .OrderByDescending(o => o.CreatedAt) // Mới nhất lên đầu
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 5. Trả về kết quả
            return (orders, totalCount);
        }
    }
}

