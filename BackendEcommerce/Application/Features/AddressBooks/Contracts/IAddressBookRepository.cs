using BackendEcommerce.Infrastructure.Persistence.Models;

namespace BackendEcommerce.Application.Features.AddressBooks.Contracts
{
    public interface IAddressBookRepository
    {
        Task<List<AddressBook>> GetByUserIdAsync(int userId);
        Task<AddressBook?> GetByIdAndUserIdAsync(int id, int userId);
        Task<AddressBook?> GetDefaultByUserIdAsync(int userId);

        Task AddAsync(AddressBook address);
        void Update(AddressBook address);
        void Delete(AddressBook address);

        Task SaveChangesAsync(); // (Cần cho Transaction (Giao dịch))
    }
}
