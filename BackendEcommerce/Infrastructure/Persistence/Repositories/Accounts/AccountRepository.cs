using BackendEcommerce.Domain.Contracts.Persistence;
using BackendEcommerce.Infrastructure.Persistence.Data;
using BackendEcommerce.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace BackendEcommerce.Infrastructure.Persistence.Repositories.Accounts
{
    public class AccountRepository : IAccountRepository
    {
        private readonly EcomDbContext _context;

        public AccountRepository(EcomDbContext context)
        {
            _context = context;
        }

        public async Task<Account?> GetByUsernameAsync(string username)
        {
            return await _context.Accounts
                .Include(a => a.User)
                .Include(r=>r.RefreshTokens)
                .FirstOrDefaultAsync(a => a.Username == username);
        }
        public Task SaveChangesAsync() => _context.SaveChangesAsync();
        public async Task<Account?> GetByEmailAsync(string email)
        {
            return await _context.Accounts
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.User.Email == email);
        }
        public async Task AddAsync(Account account)
        {
            await _context.Accounts.AddAsync(account);
        }
    }
}
