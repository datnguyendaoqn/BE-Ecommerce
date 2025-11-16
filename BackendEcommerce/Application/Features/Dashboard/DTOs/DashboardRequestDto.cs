using System;
using Microsoft.AspNetCore.Mvc;

namespace BackendEcommerce.Application.Features.Dashboard.DTOs
{
     // DTO cho các request chỉ cần khoảng thời gian
    public class DashboardDateRangeRequest
    {
        [FromQuery(Name = "from")]
        public DateTime From { get; set; } = DateTime.Now.AddDays(-30);

        [FromQuery(Name = "to")]
        public DateTime To { get; set; } = DateTime.Now;
    }

    // DTO cho request top sản phẩm
    public class TopProductsRequest : DashboardDateRangeRequest
    {
        [FromQuery(Name = "topN")]
        public int TopN { get; set; } = 5;
    }

    // DTO cho request đơn hàng gần đây
    public class RecentOrdersRequest
    {
        [FromQuery(Name = "count")]
        public int Count { get; set; } = 10;
    }
}