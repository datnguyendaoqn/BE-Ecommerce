using BackendEcommerce.Infrastructure.Persistence.Models;

namespace BackendEcommerce.Domain.Auth
{
    public class UserAuthenticator
    {
        private readonly PasswordHasher _hasher;

        public UserAuthenticator(PasswordHasher hasher)
        {
            _hasher = hasher;
        }

        public bool ValidateCredentials(Account? account, string password)
        {
            if (account == null) return false;
            return _hasher.Verify(password, account.PasswordHash);
        }
    }
}
