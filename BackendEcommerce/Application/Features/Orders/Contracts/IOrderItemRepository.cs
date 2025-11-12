using BackendEcommerce.Infrastructure.Persistence.Models;

namespace BackendEcommerce.Application.Features.Orders.Contracts
{
    public interface IOrderItemRepository
    {
        Task AddRangeAsync(IEnumerable<OrderItem> orderItems);
        Task SaveChangesAsync();
    }
}
