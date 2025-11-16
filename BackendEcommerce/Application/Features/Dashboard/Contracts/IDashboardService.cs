using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendEcommerce.Application.Features.Dashboard.DTOs;
namespace BackendEcommerce.Application.Features.Dashboard.Contracts
{
    public interface IDashboardService
    {
        /// <summary>
        /// Lấy các chỉ số tổng quan (doanh thu, đơn hàng, khách hàng mới).
        /// </summary>
        Task<DashboardSummaryResponse> GetDashboardSummaryAsync(DateTime from, DateTime to);

        /// <summary>
        /// Lấy dữ liệu doanh thu và đơn hàng nhóm theo ngày.
        /// </summary>
        Task<IEnumerable<SalesOverTimeResponse>> GetSalesOverTimeAsync(DateTime from, DateTime to);

        /// <summary>
        /// Lấy Top N sản phẩm bán chạy nhất.
        /// </summary>
        Task<IEnumerable<TopProductResponse>> GetTopSellingProductsAsync(DateTime from, DateTime to, int topN = 5);

        /// <summary>
        /// Lấy dữ liệu doanh thu theo danh mục.
        /// </summary>
        Task<IEnumerable<CategorySalesResponse>> GetSalesByCategoryAsync(DateTime from, DateTime to);

        /// <summary>
        /// Lấy N đơn hàng gần đây nhất.
        /// </summary>
        Task<IEnumerable<RecentOrderResponse>> GetRecentOrdersAsync(int count = 10);
    }
}