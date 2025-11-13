using System;

namespace BackendEcommerce.Application.Features.Dashboard.DTOs
{
    public class DashboardSummaryDTO
    {
        public decimal TotalRevenue { get; set; }  // Tổng doanh thu (từ các đơn hàng hợp lệ)
        public int TotalValidOrders { get; set; } // Tổng số đơn hàng (hợp lệ)
        public int TotalUnitsSold { get; set; }   // Tổng số sản phẩm đã bán
        public int NewCustomers { get; set; }     // Số lượng khách hàng mới
    }

        /// <summary>
        /// DTO cho biểu đồ doanh thu/đơn hàng theo thời gian (ví dụ: theo ngày).
        /// </summary>
        public class SalesOverTimeDTO
        {
            public DateOnly Date { get; set; }      // Mốc thời gian (ví dụ: ngày)
            public decimal Revenue { get; set; }    // Doanh thu trong ngày đó
            public int OrderCount { get; set; }   // Số đơn hàng trong ngày đó
        }

        /// <summary>
        /// DTO cho top sản phẩm bán chạy.
        /// </summary>
        public class TopProductDTO
        {
            public long ProductId { get; set; }         // ID từ bảng PRODUCTS
            public string ProductName { get; set; }     // Name từ bảng PRODUCTS
            public int UnitsSold { get; set; }          // Tổng Quantity từ ORDER_ITEMS
            public decimal TotalRevenue { get; set; }   // Tổng (Quantity * UnitPrice)
        }

        /// <summary>
        /// DTO cho doanh thu theo danh mục (dùng cho biểu đồ tròn).
        /// </summary>
        public class CategorySalesDTO
        {
            public string CategoryName { get; set; }    // Name từ bảng CATEGORIES
            public decimal TotalRevenue { get; set; }   // Tổng doanh thu
            public int UnitsSold { get; set; }          // Tổng số sản phẩm đã bán
        }

    /// <summary>
    /// DTO cho danh sách các đơn hàng gần đây.
    /// </summary>
    public class RecentOrderDTO
    {
        public long OrderId { get; set; }           // ID từ bảng ORDERS
        public string CustomerName { get; set; }    // FULL_NAME từ bảng USERS
        public DateTime OrderDate { get; set; }     // CREATED_AT từ bảng ORDERS
        public decimal TotalAmount { get; set; }    // TOTAL từ bảng ORDERS
        public string Status { get; set; }          // STATUS từ bảng ORDERS
    }
}