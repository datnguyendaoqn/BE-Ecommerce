using BackendEcommerce.Infrastructure.Persistence.Models;

namespace BackendEcommerce.Domain.Contracts.Persistence
{
    public interface IMediaRepository
    {
        Task AddRangeAsync(IEnumerable<Media> MediaItems);
        Task SaveChangesAsync();
        Task<Dictionary<int, string>> GetPrimaryMediaForEntitiesAsync(IEnumerable<int> entityIds, string entityType);
        Task<IReadOnlyList<Media>> GetMediaForEntityAsync(int entityId, string entityType);
        Task<Dictionary<int, Media>> GetPrimaryMediaForEntitiesMapAsync(IEnumerable<int> entityIds, string entityType);
        Task<Media?> GetByIdAsync(int mediaId);
        void Update(Media media);
    }
}
