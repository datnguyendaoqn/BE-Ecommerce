using BackendEcommerce.Application.Features.Orders.DTOs;
using BackendEcommerce.Application.Shared.DTOs;

namespace BackendEcommerce.Application.Features.Orders.Contracts
{
    public interface IOrderService
    {
        /// <summary>
        /// API (Giao diện Lập trình Ứng dụng) [POST] /api/orders
        /// (ĐÃ CẬP NHẬT (UPDATED): Logic (Lô-gic) "Tách Đơn hàng (Order)" (Split Order) Đa Cửa hàng (Multi-Shop))
        /// </summary>
        Task<ApiResponseDTO<CreateOrderResponseDto>> CreateOrderAsync(int customerId, CreateOrderRequestDto dto);

        // (Sau này thêm: GetMyOrdersAsync, GetOrderDetailAsync, CancelOrderAsync...)
    }
}
