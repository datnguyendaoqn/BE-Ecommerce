using BackendEcommerce.Domain.Contracts.Persistence;
using BackendEcommerce.Infrastructure.Persistence.Data;
using BackendEcommerce.Infrastructure.Persistence.Models;
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
    }
}
