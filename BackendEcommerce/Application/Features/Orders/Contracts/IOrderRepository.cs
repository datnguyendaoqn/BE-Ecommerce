using BackendEcommerce.Infrastructure.Persistence.Models;

namespace BackendEcommerce.Application.Features.Orders.Contracts
{
    public interface IOrderRepository
    {
        Task AddAsync(Order order);

        // (Chúng ta cần SaveChangesAsync riêng biệt
        //  để lấy OrderId (ID Đơn hàng) MỚI trước khi tạo (create) OrderItem)
        Task SaveChangesAsync();

        // (Sau này thêm các hàm Get (Lấy), Update (Cập nhật)...)
    }
}
