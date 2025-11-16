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
        public async Task<Order?> GetOrderByIdWithItemsAsync(int orderId)
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
        public async Task<(IEnumerable<CustomerOrderResponseDto> Orders, int TotalCount)> GetOrdersByUserIdAsync(
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

            // 3. Đếm tổng số lượng (dùng cho phân trang)
            var totalCount = await query.CountAsync();

            // 4. Áp dụng Sắp xếp, Phân trang và PROJECTION (Phần quan trọng nhất)
            // Bỏ .Include() và dùng .Select()
            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new CustomerOrderResponseDto
                {
                    // Map các trường của Order
                    Id = o.Id,
                    ShopId = o.ShopId,
                    ShopName = o.Shop.Name, // EF Core sẽ tự động join bảng Shop khi cần
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,
                    PaymentMethod = o.PaymentMethod,
                    CreatedAt = o.CreatedAt,

                    // TỐI ƯU 1: Đếm tổng số item con ngay tại DB
                    TotalItemsCount = o.OrderItems.Count(),

                    // TỐI ƯU 2: Chỉ lấy 2 item đầu tiên từ DB
                    Items = o.OrderItems
                        .OrderBy(i => i.Id) // Sắp xếp để lấy 2 item đầu tiên
                        .Take(2) // Chỉ lấy 2
                        .Select(i => new CustomerOrderItemDto
                        {
                            ProductName = i.ProductName,
                            VariantName = i.VariantName,
                            Sku = i.Sku,
                            Quantity = i.Quantity,
                            Price = i.PriceAtTimeOfPurchase,
                            ImageUrl = i.ImageUrl
                        }).ToList()
                })
                .ToListAsync();

            // 5. Trả về
            return (orders, totalCount);
        }
    }
}


