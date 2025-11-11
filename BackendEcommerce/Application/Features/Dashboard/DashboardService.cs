using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using BackendEcommerce.Application.Features.Dashboard.Contracts;
using BackendEcommerce.Application.Features.Dashboard.DTOs;
using BackendEcommerce.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using BackendEcommerce.Infrastructure.Persistence.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace BackendEcommerce.Application.Features.Dashboard
{
  public class DashboardService : IDashboardService // Triển khai interface từ namespace mới
    {
        private readonly EcomDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor; // Thêm

        public DashboardService(EcomDbContext context, IHttpContextAccessor httpContextAccessor) // Cập nhật constructor
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor; // Thêm
        }

        /// <summary>
        /// Lấy Shop ID của seller đang đăng nhập từ HttpContext.
        /// Đồng thời xác thực vai trò "seller".
        /// </summary>
        private async Task<long> GetCurrentSellerShopIdAsync()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !user.Identity.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            // 1. Kiểm tra vai trò "seller" (Giả sử role được lưu trong Claim)
            if (!user.IsInRole("seller"))
            {
                throw new UnauthorizedAccessException("User does not have the 'seller' role.");
            }

            // 2. Lấy User ID từ Claim (Giả sử ID được lưu trong NameIdentifier)
            var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !long.TryParse(userIdString, out long userId))
            {
                throw new UnauthorizedAccessException("Authenticated user ID is not found.");
            }

            // 3. Tìm Shop tương ứng của User này
            // (Giả định 1 user (seller) chỉ sở hữu 1 shop)
            var shopId = await _context.Shops
                .AsNoTracking()
                .Where(s => s.OwnerId == userId)
                .Select(s => s.Id)
                .FirstOrDefaultAsync();

            if (shopId == 0)
            {
                // Có role "seller" nhưng không tìm thấy shop (ví dụ: shop chưa được duyệt)
                throw new UnauthorizedAccessException("User is a seller but no associated shop was found.");
            }
            
            return shopId;
        }

        /// <summary>
        /// Bộ lọc (Query) cơ sở cho các MỤC ĐƠN HÀNG HỢP LỆ (đã hoàn thành, không bị hủy)
        /// VÀ CHỈ THUỘC VỀ SHOP ĐƯỢC CHỈ ĐỊNH.
        /// </summary>
        private IQueryable<OrderItem> GetValidOrderItems(long shopId)
        {
            // GIẢ ĐỊNH: Đơn hàng hợp lệ là đơn KHÔNG 'pending' VÀ KHÔNG có lý do hủy
            return _context.OrderItems
                .AsNoTracking() // Quan trọng: Tăng hiệu suất cho truy vấn chỉ đọc
                .Where(oi => oi.Order.Status != "pending" 
                             && oi.Order.CancellationReason == null
                             && oi.Variant.Product.ShopId == shopId); // <-- LỌC THEO SHOP ID
        }

        public async Task<DashboardSummaryDTO> GetDashboardSummaryAsync(DateTime from, DateTime to)
        {
            long shopId = await GetCurrentSellerShopIdAsync(); // Xác thực và lấy Shop ID
            
            var itemsInDateRange = GetValidOrderItems(shopId) // Chỉ lấy items của shop
                .Where(oi => oi.Order.CreatedAt >= from && oi.Order.CreatedAt <= to);

            // Tính toán song song các chỉ số
            var revenueTask = itemsInDateRange.SumAsync(oi => oi.Quantity * oi.UnitPrice);
            var unitsSoldTask = itemsInDateRange.SumAsync(oi => oi.Quantity);
            var ordersCountTask = itemsInDateRange.Select(oi => oi.OrderId).Distinct().CountAsync();
            
            // Tính khách hàng mới (của toàn platform, 1 KPI chung)
            // Seller có thể muốn biết platform có user mới không
            var newCustomersTask = _context.Users
                .AsNoTracking()
                .CountAsync(u => u.Role == "customer" && u.CreatedAt >= from && u.CreatedAt <= to);
            
            // Chờ tất cả hoàn thành
            await Task.WhenAll(revenueTask, unitsSoldTask, ordersCountTask, newCustomersTask);

            return new DashboardSummaryDTO
            {
                TotalRevenue = revenueTask.Result,
                TotalUnitsSold = unitsSoldTask.Result,
                TotalValidOrders = ordersCountTask.Result,
                NewCustomers = newCustomersTask.Result
            };
        }

        public async Task<IEnumerable<SalesOverTimeDTO>> GetSalesOverTimeAsync(DateTime from, DateTime to)
        {
            long shopId = await GetCurrentSellerShopIdAsync();

            var salesByDay = await GetValidOrderItems(shopId) // Chỉ lấy items của shop
                .Where(oi => oi.Order.CreatedAt >= from && oi.Order.CreatedAt <= to)
                .GroupBy(oi => oi.Order.CreatedAt.Date) // Nhóm theo Ngày
                .Select(g => new SalesOverTimeDTO
                {
                    Date = DateOnly.FromDateTime(g.Key),
                    Revenue = g.Sum(oi => oi.Quantity * oi.UnitPrice),
                    OrderCount = g.Select(oi => oi.OrderId).Distinct().Count()
                })
                .OrderBy(dto => dto.Date)
                .ToListAsync();

            return salesByDay;
        }

        public async Task<IEnumerable<TopProductDTO>> GetTopSellingProductsAsync(DateTime from, DateTime to, int topN = 5)
        {
            long shopId = await GetCurrentSellerShopIdAsync();
            
            var topProducts = await GetValidOrderItems(shopId) // Chỉ lấy items của shop
                .Where(oi => oi.Order.CreatedAt >= from && oi.Order.CreatedAt <= to)
                .GroupBy(oi => oi.Variant.Product) // Nhóm theo Product
                .Select(g => new TopProductDTO
                {
                    ProductId = g.Key.Id,
                    ProductName = g.Key.Name,
                    UnitsSold = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                })
                .OrderByDescending(dto => dto.TotalRevenue)
                .Take(topN)
                .ToListAsync();
            
            return topProducts;
        }

        public async Task<IEnumerable<CategorySalesDTO>> GetSalesByCategoryAsync(DateTime from, DateTime to)
        {
            long shopId = await GetCurrentSellerShopIdAsync();
            
            var categorySales = await GetValidOrderItems(shopId) // Chỉ lấy items của shop
                .Where(oi => oi.Order.CreatedAt >= from && oi.Order.CreatedAt <= to && oi.Variant.Product.Category != null)
                .GroupBy(oi => oi.Variant.Product.Category) // Nhóm theo Category
                .Select(g => new CategorySalesDTO
                {
                    CategoryName = g.Key.Name,
                    TotalRevenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                })
                .OrderByDescending(dto => dto.TotalRevenue)
                .ToListAsync();

            return categorySales;
        }

        public async Task<IEnumerable<RecentOrderDTO>> GetRecentOrdersAsync(int count = 10)
        {
            long shopId = await GetCurrentSellerShopIdAsync();
            
            // Sửa lại logic: Lấy các đơn hàng GẦN ĐÂY CÓ CHỨA SẢN PHẨM CỦA SHOP
            var recentOrdersForShop = await _context.OrderItems
                .AsNoTracking()
                .Where(oi => oi.Variant.Product.ShopId == shopId) // Chỉ lấy items của shop
                .GroupBy(oi => oi.Order) // Nhóm theo Order
                .OrderByDescending(g => g.Key.CreatedAt) // Sắp xếp theo ngày tạo Order
                .Take(count)
                .Select(g => new RecentOrderDTO
                {
                    OrderId = g.Key.Id,
                    CustomerName = (g.Key.User != null) ? g.Key.User.FullName : "N/A",
                    OrderDate = g.Key.CreatedAt,
                    // Tính tổng tiền CHỈ CỦA CÁC SẢN PHẨM THUỘC SHOP NÀY trong đơn hàng đó
                    TotalAmount = g.Sum(oi => oi.Quantity * oi.UnitPrice), 
                    Status = g.Key.Status
                })
                .ToListAsync();
            
            return recentOrdersForShop;
        }
    }
}