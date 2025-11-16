// DTO cho các thẻ tổng quan
    public class DashboardSummaryResponse
    {
        public decimal TotalRevenue { get; set; }
        public int TotalValidOrders { get; set; }
        public int TotalUnitsSold { get; set; }
        public int NewCustomers { get; set; }
    }

    // DTO cho biểu đồ doanh thu theo thời gian
    public class SalesOverTimeResponse
    {
        public string Date { get; set; } // Trả về string 'YYYY-MM-DD'
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    // DTO cho top sản phẩm bán chạy
    public class TopProductResponse
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public int UnitsSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    // DTO cho doanh thu theo danh mục
    public class CategorySalesResponse
    {
        public string CategoryName { get; set; }
        public int UnitsSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    // DTO cho các đơn hàng gần đây
    public class RecentOrderResponse
    {
        public long OrderId { get; set; }
        public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
    }