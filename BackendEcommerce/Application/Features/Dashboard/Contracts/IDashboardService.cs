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
        Task<DashboardSummaryDTO> GetDashboardSummaryAsync(DateTime from, DateTime to);

        /// <summary>
        /// Lấy dữ liệu doanh thu và đơn hàng nhóm theo ngày.
        /// </summary>
        Task<IEnumerable<SalesOverTimeDTO>> GetSalesOverTimeAsync(DateTime from, DateTime to);

        /// <summary>
        /// Lấy Top N sản phẩm bán chạy nhất.
        /// </summary>
        Task<IEnumerable<TopProductDTO>> GetTopSellingProductsAsync(DateTime from, DateTime to, int topN = 5);

        /// <summary>
        /// Lấy dữ liệu doanh thu theo danh mục.
        /// </summary>
        Task<IEnumerable<CategorySalesDTO>> GetSalesByCategoryAsync(DateTime from, DateTime to);

        /// <summary>
        /// Lấy N đơn hàng gần đây nhất.
        /// </summary>
        Task<IEnumerable<RecentOrderDTO>> GetRecentOrdersAsync(int count = 10);
    }
}