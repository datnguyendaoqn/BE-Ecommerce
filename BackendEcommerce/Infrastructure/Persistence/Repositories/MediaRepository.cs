using BackendEcommerce.Application.Features.Medias.Contracts;
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

        public async Task<Media?> GetByIdAsync(int mediaId)
        {
            return await _context.Media
                .FirstOrDefaultAsync(m => m.Id == mediaId);
        }

        public void Update(Media media)
        {
            // Chỉ cần đánh dấu là "Modified", SaveChangesAsync sẽ lưu sau
            _context.Entry(media).State = EntityState.Modified;
        }
        public void Delete(Media media)
        {
            // Chỉ cần đánh dấu là "Deleted", SaveChangesAsync sẽ xóa sau
            _context.Media.Remove(media);
        }
        /// <summary>
        /// (Mới - Tối ưu) Lấy 1 ảnh duy nhất của Variant
        /// </summary>
        public async Task<Media?> GetMediaForVariantAsync(int variantId)
        {
            // Bỏ AsNoTracking() vì chúng ta có thể Delete
            return await _context.Media
                .FirstOrDefaultAsync(m => m.EntityId == variantId && m.EntityType == "variant");
        }

        /// <summary>
        /// (Mới - Sửa lỗi) Xóa nhiều Media item (Xóa Cứng)
        /// </summary>
        public void DeleteRange(IEnumerable<Media> mediaItems)
        {
            _context.Media.RemoveRange(mediaItems);
        }
    }
}
