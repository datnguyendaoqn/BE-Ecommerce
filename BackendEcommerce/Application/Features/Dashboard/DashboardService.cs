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

    var itemsInDateRange = GetValidOrderItems(shopId)
        .Where(oi => oi.Order.CreatedAt >= from && oi.Order.CreatedAt <= to);

    // --- BẮT ĐẦU SỬA LỖI ---
    // Chúng ta phải chạy các truy vấn này một cách TUẦN TỰ (sequential)
    // để tránh lỗi "DbContext" không thread-safe.

    // 1. Tính toán doanh thu và đơn vị bán được (có thể gộp)
    var shopStats = await itemsInDateRange
        .GroupBy(oi => 1) // Nhóm tất cả thành 1 nhóm
        .Select(g => new
        {
            TotalRevenue = g.Sum(oi => oi.Quantity * oi.PriceAtTimeOfPurchase),
            TotalUnitsSold = g.Sum(oi => oi.Quantity)
        })
        .FirstOrDefaultAsync();

    // 2. Đếm số đơn hàng riêng biệt
    var ordersCount = await itemsInDateRange
        .Select(oi => oi.OrderId)
        .Distinct()
        .CountAsync();

    // 3. Đếm khách hàng mới (của toàn platform)
    // Phải dùng _context trực tiếp vì GetValidOrderItems đã lọc theo shop
    var newCustomers = await _context.Users
        .AsNoTracking()
        .CountAsync(u => u.Role == "customer" && u.CreatedAt >= from && u.CreatedAt <= to);
        
    // --- KẾT THÚC SỬA LỖI ---

    return new DashboardSummaryDTO
    {
        // Dùng FirstOrDefaultAsync ở trên có thể trả về null nếu không có đơn hàng nào
        TotalRevenue = shopStats?.TotalRevenue ?? 0,
        TotalUnitsSold = shopStats?.TotalUnitsSold ?? 0,
        TotalValidOrders = ordersCount,
        NewCustomers = newCustomers
    };
}

    public async Task<IEnumerable<SalesOverTimeDTO>> GetSalesOverTimeAsync(DateTime from, DateTime to)
{
    long shopId = await GetCurrentSellerShopIdAsync();

    // 1. Truy vấn và nhóm (GroupBy) bằng cách sử dụng g.Key là DateTime (EF Core dịch được)
    var salesByDay_Anonymous = await GetValidOrderItems(shopId)
        .Where(oi => oi.Order.CreatedAt >= from && oi.Order.CreatedAt <= to)
        .GroupBy(oi => oi.Order.CreatedAt.Date) // EF Core có thể dịch .Date
        .Select(g => new
        {
            DateKey = g.Key, // g.Key ở đây vẫn là DateTime
            Revenue = g.Sum(oi => oi.Quantity * oi.PriceAtTimeOfPurchase),
            OrderCount = g.Select(oi => oi.OrderId).Distinct().Count()
        })
        .OrderBy(x => x.DateKey)
        .ToListAsync(); // <-- 2. Lấy dữ liệu về bộ nhớ máy chủ (Client)

    // 3. Khi dữ liệu đã ở trên máy chủ, ta mới dùng .Select() của LINQ-to-Objects
    // để chuyển đổi DateTime (DateKey) sang DateOnly an toàn.
    var salesByDay = salesByDay_Anonymous
        .Select(g => new SalesOverTimeDTO
        {
            Date = DateOnly.FromDateTime(g.DateKey), // <-- Hàm này giờ chạy trong C#, không phải SQL
            Revenue = g.Revenue,
            OrderCount = g.OrderCount
        });

    return salesByDay;
}
       public async Task<IEnumerable<TopProductDTO>> GetTopSellingProductsAsync(DateTime from, DateTime to, int topN = 5)
{
    long shopId = await GetCurrentSellerShopIdAsync();
    
    
    // GroupBy các trường cụ thể (Id và Name) mà chúng ta cần.
    var topProducts = await GetValidOrderItems(shopId)
        .Where(oi => oi.Order.CreatedAt >= from && oi.Order.CreatedAt <= to)
        // Nhóm theo một đối tượng ẩn danh (anonymous object) chỉ chứa các kiểu đơn giản
        .GroupBy(oi => new { 
            oi.Variant.Product.Id, 
            oi.Variant.Product.Name 
        }) 
        .Select(g => new TopProductDTO
        {
            ProductId = g.Key.Id,       // Lấy từ Key của GroupBy
            ProductName = g.Key.Name,   // Lấy từ Key của GroupBy
            UnitsSold = g.Sum(oi => oi.Quantity),
            TotalRevenue = g.Sum(oi => oi.Quantity * oi.PriceAtTimeOfPurchase)
        })
        .OrderByDescending(dto => dto.TotalRevenue) // Order sau khi Select
        .Take(topN)
        .ToListAsync();
    
    return topProducts;
}
        public async Task<IEnumerable<CategorySalesDTO>> GetSalesByCategoryAsync(DateTime from, DateTime to)
        {
            long shopId = await GetCurrentSellerShopIdAsync();
            
            var categorySales = await GetValidOrderItems(shopId) // Chỉ lấy items của shop
                .Where(oi => oi.Order.CreatedAt >= from && oi.Order.CreatedAt <= to && oi.Variant.Product.Category != null)
                .GroupBy(oi => new 
                { 
                    oi.Variant.Product.Category.Name 
                })
                .Select(g => new CategorySalesDTO
                {
                    CategoryName = g.Key.Name,
                    UnitsSold = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.Quantity * oi.PriceAtTimeOfPurchase)
                })
                .OrderByDescending(dto => dto.TotalRevenue)
                .ToListAsync();

            return categorySales;
        }

        public async Task<IEnumerable<RecentOrderDTO>> GetRecentOrdersAsync(int count = 10)
{
    long shopId = await GetCurrentSellerShopIdAsync();

    // SỬA LỖI ORA-12704:
    // 1. GroupBy và Select các cột GỐC (raw) vào một kiểu
    //    trung gian (anonymous type) TRƯỚC KHI gọi ToListAsync.
    var intermediateData = await _context.OrderItems
        .AsNoTracking()
        .Where(oi => oi.Variant.Product.ShopId == shopId)
        .GroupBy(oi => new {
            oi.Order.Id,
            oi.Order.User.FullName, // <-- Group by cột gốc (có thể null)
            oi.Order.CreatedAt,
            oi.Order.Status
        })
        .OrderByDescending(g => g.Key.CreatedAt)
        .Take(count)
        .Select(g => new 
        {
            // Lấy dữ liệu thô từ g.Key
            g.Key.Id,
            g.Key.FullName, // <-- Lấy FullName (có thể null)
            g.Key.CreatedAt,
            g.Key.Status,
            // Tính tổng (việc này an toàn trong SQL)
            TotalAmount = g.Sum(oi => oi.Quantity * oi.PriceAtTimeOfPurchase)
        })
        .ToListAsync(); // <-- Dữ liệu được đưa vào bộ nhớ C# TẠI ĐÂY

    // 2. Bây giờ, dữ liệu đã ở trong C#. Chúng ta dùng LINQ-to-Objects
    //    để áp dụng logic "??" một cách an toàn mà không bị dịch sang SQL.
    var recentOrdersForShop = intermediateData.Select(g => new RecentOrderDTO
    {
        OrderId = g.Id,
        CustomerName = g.FullName ?? "N/A", // <-- Logic này được C# xử lý
        OrderDate = g.CreatedAt,
        TotalAmount = g.TotalAmount,
        Status = g.Status
    });

    return recentOrdersForShop;
}
    }
}