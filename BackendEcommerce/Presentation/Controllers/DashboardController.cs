using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendEcommerce.Application.Features.Dashboard.Contracts;
using BackendEcommerce.Application.Features.Dashboard.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendEcommerce.Presentation.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize(Roles = "seller")] // <-- CHỈ CẦN DÒNG NÀY ĐỂ BẢO VỆ TOÀN BỘ API
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        /// <summary>
        /// Helper private để xử lý logic gọi service và bắt lỗi chung
        /// </summary>
        private async Task<IActionResult> ExecuteServiceCall<T>(Func<Task<T>> serviceCall)
        {
            try
            {
                var result = await serviceCall();
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                // Bắt lỗi nếu ICurrentSellerService không tìm thấy shop hoặc user không phải seller
                return Forbid(ex.Message); // Trả về 403 Forbidden
            }
            catch (Exception ex)
            {
                // Bắt các lỗi chung khác (ví dụ: lỗi CSDL)
                return StatusCode(500, new { message = "An internal server error occurred.", details = ex.Message });
            }
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetDashboardSummary(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            // Mặc định là 30 ngày qua nếu không cung cấp
            DateTime toDate = to ?? DateTime.UtcNow;
            DateTime fromDate = from ?? toDate.AddDays(-30);

            return await ExecuteServiceCall(() =>
                _dashboardService.GetDashboardSummaryAsync(fromDate, toDate)
            );
        }

        [HttpGet("sales-over-time")]
        public async Task<IActionResult> GetSalesOverTime(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            DateTime toDate = to ?? DateTime.UtcNow;
            DateTime fromDate = from ?? toDate.AddDays(-30);

            return await ExecuteServiceCall(() =>
                _dashboardService.GetSalesOverTimeAsync(fromDate, toDate)
            );
        }

        [HttpGet("top-products")]
        public async Task<IActionResult> GetTopSellingProducts(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int topN = 5)
        {
            DateTime toDate = to ?? DateTime.UtcNow;
            DateTime fromDate = from ?? toDate.AddDays(-30);

            return await ExecuteServiceCall(() =>
                _dashboardService.GetTopSellingProductsAsync(fromDate, toDate, topN)
            );
        }

        [HttpGet("sales-by-category")]
        public async Task<IActionResult> GetSalesByCategory(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            DateTime toDate = to ?? DateTime.UtcNow;
            DateTime fromDate = from ?? toDate.AddDays(-30);

            return await ExecuteServiceCall(() =>
                _dashboardService.GetSalesByCategoryAsync(fromDate, toDate)
            );
        }

        [HttpGet("recent-orders")]
        public async Task<IActionResult> GetRecentOrders([FromQuery] int count = 10)
        {
            return await ExecuteServiceCall(() =>
                _dashboardService.GetRecentOrdersAsync(count)
            );
        }
    }
}    