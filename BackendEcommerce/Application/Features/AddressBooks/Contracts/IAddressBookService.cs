using BackendEcommerce.Application.Features.AddressBooks.DTOs;
using BackendEcommerce.Application.Shared.DTOs;

namespace BackendEcommerce.Application.Features.AddressBooks.Contracts
{
    public interface IAddressBookService
    {
        Task<ApiResponseDTO<List<AddressBookDto>>> GetMyAddressesAsync(int customerId);

        Task<ApiResponseDTO<AddressBookDto>> CreateAddressAsync(int customerId, CreateAddressBookRequestDto dto);

        Task<ApiResponseDTO<AddressBookDto>> UpdateAddressAsync(int addressId, int customerId, UpdateAddressBookRequestDto dto);

        Task<ApiResponseDTO<string>> DeleteAddressAsync(int addressId, int customerId);

        Task<ApiResponseDTO<string>> SetDefaultAddressAsync(int addressId, int customerId);
    }
}
