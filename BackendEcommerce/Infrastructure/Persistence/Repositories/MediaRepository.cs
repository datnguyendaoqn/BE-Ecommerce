using BackendEcommerce.Domain.Contracts.Persistence;
using BackendEcommerce.Infrastructure.Persistence.Data;
using BackendEcommerce.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
namespace BackendEcommerce.Infrastructure.Persistence.Repositories
{
    public class MediaRepository : IMediaRepository
    {
        private readonly EcomDbContext _context;

        public MediaRepository(EcomDbContext context)
        {
            _context = context;
        }

        public async Task AddRangeAsync(IEnumerable<Media> mediaItems)
        {
            await _context.Media.AddRangeAsync(mediaItems);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<Dictionary<int, string>> GetPrimaryMediaForEntitiesAsync(IEnumerable<int> entityIds, string entityType)
        {
            // Chạy 1 query duy nhất để lấy tất cả ảnh chính
            // Đây là giải pháp "Production-Ready" để chống N+1 Query
            return await _context.Media
                .Where(m => m.EntityId.HasValue &&
                            entityIds.Contains(m.EntityId.Value) &&
                            m.EntityType == entityType &&
                            m.IsPrimary) // Chỉ lấy ảnh chính
                .GroupBy(m => m.EntityId.Value) // Nhóm theo ProductId
                .Select(g => new {
                    EntityId = g.Key,
                    // Lấy ảnh đầu tiên (vì có thể có nhiều ảnh IsPrimary, dù không nên)
                    ImageUrl = g.First().ImageUrl
                })
                .ToDictionaryAsync(k => k.EntityId, v => v.ImageUrl!);
        }
        public async Task<IReadOnlyList<Media>> GetMediaForEntityAsync(int entityId, string entityType)
        {
            return await _context.Media
                .Where(m => m.EntityId == entityId && m.EntityType == entityType)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Dictionary<int, Media>> GetPrimaryMediaForEntitiesMapAsync(IEnumerable<int> entityIds, string entityType)
        {
            // Gần giống hàm GetPrimaryMediaForEntitiesAsync,
            // nhưng trả về nguyên object Media
            return await _context.Media
                .Where(m => m.EntityId.HasValue &&
                            entityIds.Contains(m.EntityId.Value) &&
                            m.EntityType == entityType &&
                            m.IsPrimary)
                .GroupBy(m => m.EntityId.Value)
                .Select(g => g.First()) // Lấy object Media đầu tiên
                .ToDictionaryAsync(k => k.EntityId.Value);
        }
    }
}
