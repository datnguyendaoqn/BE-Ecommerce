using BackendEcommerce.Infrastructure.Persistence.Models;

namespace BackendEcommerce.Domain.Contracts.Persistence
{
    public interface IMediaRepository
    {
        Task AddRangeAsync(IEnumerable<Media> MediaItems);
        Task SaveChangesAsync();
        Task<Dictionary<int, string>> GetPrimaryMediaForEntitiesAsync(IEnumerable<int> entityIds, string entityType);
    }
}
