using BackendEcommerce.Application.Features.AddressBooks.Contracts;
using BackendEcommerce.Application.Features.AddressBooks.DTOs;
using BackendEcommerce.Application.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackendEcommerce.Presentation.Controllers
{
    /// <summary>
    /// API CỐT LÕI (CORE) - Dùng để Quản lý "Sổ Địa chỉ"
    /// (Điều kiện tiên quyết (Prerequisite) cho "Đặt hàng" (Checkout))
    /// </summary>
    [Route("api/addresses")]
    [ApiController]
    [Authorize] // (BẮT BUỘC Đăng nhập)
    public class AddressBookController : ControllerBase
    {
        private readonly IAddressBookService _addressService;

        public AddressBookController(IAddressBookService addressService)
        {
            _addressService = addressService;
        }

        // (Hàm trợ giúp lấy CustomerId từ Token)
        private int GetCurrentCustomerId()
        {
            var customerIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // (Nên có check Parse an toàn hơn)
            return int.Parse(customerIdString!);
        }

        /// <summary>
        /// [API 1] Lấy TẤT CẢ địa chỉ của tôi
        /// (Dùng cho Trang Checkout (Thanh toán) / Trang Quản lý Sổ Địa chỉ (Address Book))
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponseDTO<List<AddressBookDto>>>> GetMyAddresses()
        {
            var customerId = GetCurrentCustomerId();
            var response = await _addressService.GetMyAddressesAsync(customerId);
            return Ok(response);
        }

        /// <summary>
        /// [API 2] Tạo địa chỉ mới
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponseDTO<AddressBookDto>>> CreateAddress(
            [FromBody] CreateAddressBookRequestDto dto)
        {
            var customerId = GetCurrentCustomerId();
            var response = await _addressService.CreateAddressAsync(customerId, dto);

            if (!response.IsSuccess)
            {
                return (response.Code == 400) ? BadRequest(response) : StatusCode(500, response);
            }
            return CreatedAtAction(nameof(GetMyAddresses), new { }, response); // (Trả về (Return) 201 Created)
        }

        /// <summary>
        /// [API 3] Sửa địa chỉ
        /// </summary>
        [HttpPut("{addressId}")]
        public async Task<ActionResult<ApiResponseDTO<AddressBookDto>>> UpdateAddress(
            int addressId, [FromBody] UpdateAddressBookRequestDto dto)
        {
            var customerId = GetCurrentCustomerId();
            var response = await _addressService.UpdateAddressAsync(addressId, customerId, dto);

            if (!response.IsSuccess)
            {
                return response.Code switch
                {
                    404 => NotFound(response),
                    400 => BadRequest(response),
                    _ => StatusCode(500, response)
                };
            }
            return Ok(response);
        }

        /// <summary>
        /// [API 4] Xóa địa chỉ
        /// </summary>
        [HttpDelete("{addressId}")]
        public async Task<ActionResult<ApiResponseDTO<string>>> DeleteAddress(int addressId)
        {
            var customerId = GetCurrentCustomerId();
            var response = await _addressService.DeleteAddressAsync(addressId, customerId);

            if (!response.IsSuccess)
            {
                return response.Code switch
                {
                    404 => NotFound(response),
                    400 => BadRequest(response),
                    _ => StatusCode(500, response)
                };
            }
            return Ok(response);
        }

        /// <summary>
        /// [API 5] Đặt làm địa chỉ Mặc định (Default)
        /// </summary>
        [HttpPut("{addressId}/set-default")]
        public async Task<ActionResult<ApiResponseDTO<string>>> SetDefaultAddress(int addressId)
        {
            var customerId = GetCurrentCustomerId();
            var response = await _addressService.SetDefaultAddressAsync(addressId, customerId);

            if (!response.IsSuccess)
            {
                return response.Code switch
                {
                    404 => NotFound(response),
                    _ => StatusCode(500, response)
                };
            }
            return Ok(response);
        }
    }
}
