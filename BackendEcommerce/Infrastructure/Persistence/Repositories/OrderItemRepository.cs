using BackendEcommerce.Application.Features.Orders.Contracts;
using BackendEcommerce.Infrastructure.Persistence.Data;
using BackendEcommerce.Infrastructure.Persistence.Models;

namespace BackendEcommerce.Infrastructure.Persistence.Repositories
{
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly EcomDbContext _context;

        public OrderItemRepository(EcomDbContext context)
        {
            _context = context;
        }

        public async Task AddRangeAsync(IEnumerable<OrderItem> orderItems)
        {
            await _context.OrderItems.AddRangeAsync(orderItems);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
