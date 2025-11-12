using System.Threading.Tasks;
namespace BackendEcommerce.Application.Features.Admin.Contracts
{
    public interface IUpdateUserRoleService
    {
        Task UpdateUserRoleAsync(long userId, string newRole);
    }
}