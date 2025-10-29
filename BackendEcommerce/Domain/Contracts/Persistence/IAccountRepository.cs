using BackendEcommerce.Infrastructure.Persistence.Models;

namespace BackendEcommerce.Domain.Contracts.Persistence
{
    public interface IAccountRepository
    {
        Task<Account?> GetByUsernameAsync(string username);
        Task SaveChangesAsync();
        Task<Account?> GetByEmailAsync(string? email);
        Task AddAsync(Account account);
    }
}
