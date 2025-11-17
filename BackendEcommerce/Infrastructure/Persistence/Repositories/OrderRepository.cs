using BackendEcommerce.Application.Features.CustomerOrders.DTOs;
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
        /// <summary>
        /// (MỚI) Triển khai GetOrderByIdWithItemsAsync
        /// </summary>
        public async Task<Order?> GetOrderDetailByIdWithItemsAsync(int orderId)
        {
            // Lấy 1 đơn, Include Items
            // KHÔNG dùng AsNoTracking() vì chúng ta sẽ CẬP NHẬT nó
            return await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o=>o.Shop)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        /// <summary>
        /// (MỚI) Triển khai Update (đồng bộ)
        /// </summary>
        public void Update(Order order)
        {
            // Đánh dấu entity là đã thay đổi. EF Core sẽ biết cần phải làm gì
            // khi SaveChangesAsync() được gọi.
            _context.Entry(order).State = EntityState.Modified;
        }
        // === BỔ SUNG HÀM MỚI CHO CUSTOMER ===

        /// <summary>
        /// Triển khai hàm GetOrdersByUserIdAsync với logic Projection tối ưu
        /// </summary>
        public async Task<(IEnumerable<Order> Orders, int TotalCount)> GetOrdersByUserIdAsync(
            int userId,
            string? status,
            int pageNumber,
            int pageSize)
        {
            // 1. Query cơ sở
            var query = _context.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId);

            // 2. Filter theo Status
            if (!string.IsNullOrEmpty(status))
            {
                var statusLower = status.ToLower();
                query = query.Where(o => o.Status.ToLower() == statusLower);
            }

            // 3. Đếm tổng số lượng
            var totalCount = await query.CountAsync();

            // 4. Áp dụng Sắp xếp, Phân trang và INCLUDE
            var orders = await query
                .Include(o => o.Shop)       // <-- (THAY ĐỔI) Cần cho MapToCustomerDto
                .Include(o => o.OrderItems) // <-- (THAY ĐỔI) Cần cho MapToCustomerDto
                .OrderByDescending(o => o.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 5. Trả về
            return (orders, totalCount);
        }
        //
        // === (TRIỂN KHAI HÀM MỚI) ===
        public async Task<OrderItem?> GetOrderItemForReviewAsync(int orderItemId)
        {
            return await _context.OrderItems
                .Include(oi => oi.Order)     // Gộp 'Order' (để check UserId và Status)
                .Include(oi => oi.Variant)   // Gộp 'Variant' (để map DTO trả về)
                .FirstOrDefaultAsync(oi => oi.Id == orderItemId);
        }
    }
}


