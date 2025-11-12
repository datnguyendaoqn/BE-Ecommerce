using BackendEcommerce.Application.Features.AddressBooks.Contracts;
using BackendEcommerce.Infrastructure.Persistence.Data;
using BackendEcommerce.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendEcommerce.Infrastructure.Persistence.Repositories
{
    public class AddressBookRepository : IAddressBookRepository
    {
        private readonly EcomDbContext _context;

        public AddressBookRepository(EcomDbContext context)
        {
            _context = context;
        }

        public async Task<List<AddressBook>> GetByUserIdAsync(int userId)
        {
            // Lấy Sổ Địa chỉ (Address Book) (đã "Snapshot" (Sao chép)), sắp xếp (sort) Mặc định (Default) lên đầu
            return await _context.AddressBooks
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault) // (Mặc định lên đầu)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<AddressBook?> GetByIdAndUserIdAsync(int id, int userId)
        {
            // (Phải dùng tracking (theo dõi) (bỏ AsNoTracking) vì chúng ta sẽ Update (Cập nhật)/Delete (Xóa))
            return await _context.AddressBooks
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        }

        public async Task<AddressBook?> GetDefaultByUserIdAsync(int userId)
        {
            // (Dùng tracking (theo dõi) (bỏ AsNoTracking) vì chúng ta sẽ Update (Cập nhật) (tắt IsDefault))
            return await _context.AddressBooks
                .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault == true);
        }

        public async Task AddAsync(AddressBook address)
        {
            await _context.AddressBooks.AddAsync(address);
        }

        public void Update(AddressBook address)
        {
            _context.Entry(address).State = EntityState.Modified;
        }

        public void Delete(AddressBook address)
        {
            _context.AddressBooks.Remove(address);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
