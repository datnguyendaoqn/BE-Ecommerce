using BackendEcommerce.Application.Features.SellerRegistration.DTOs;
using System.Threading.Tasks;
using System.Security.Claims;

namespace BackendEcommerce.Application.Features.SellerRegistration.Contracts
{
    public interface ISellerRegistrationService
    {
         Task<int> RegisterSellerAsync(SellerRegistrationDTOs dto, ClaimsPrincipal User);
    }
}