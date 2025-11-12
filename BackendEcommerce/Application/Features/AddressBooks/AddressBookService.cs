using BackendEcommerce.Application.Features.AddressBooks.Contracts;
using BackendEcommerce.Application.Features.AddressBooks.DTOs;
using BackendEcommerce.Application.Features.Locations.Contracts;
using BackendEcommerce.Application.Shared.DTOs;
using BackendEcommerce.Infrastructure.Persistence.Data;
using BackendEcommerce.Infrastructure.Persistence.Models;

namespace BackendEcommerce.Application.Features.AddressBooks
{
    public class AddressBookService : IAddressBookService
    {
        private readonly IAddressBookRepository _addressRepo;
        private readonly ILocationRepository _locationRepo; // (Cần để "Snapshot" (Sao chép) Tên (Name))
        private readonly EcomDbContext _context; // (Cần cho Transaction (Giao dịch))

        public AddressBookService(
            IAddressBookRepository addressRepo,
            ILocationRepository locationRepo,
            EcomDbContext context)
        {
            _addressRepo = addressRepo;
            _locationRepo = locationRepo;
            _context = context;
        }

        // 1. API: [GET] /api/addresses
        public async Task<ApiResponseDTO<List<AddressBookDto>>> GetMyAddressesAsync(int customerId)
        {
            var addresses = await _addressRepo.GetByUserIdAsync(customerId);

            // "Ánh xạ" (Map) (siêu nhanh, không JOIN)
            var dtos = addresses.Select(a => new AddressBookDto
            {
                Id = a.Id,
                FullName = a.FullName ?? "",
                Phone = a.Phone ?? "",
                AddressLine = a.AddressLine ?? "",
                ProvinceName = a.ProvinceName, // (Đã "Snapshot" (Sao chép))
                DistrictName = a.DistrictName,
                WardName = a.WardName,
                ProvinceCode = a.ProvinceCode, // (Trả về (Return) Code (Mã) để FE (Frontend) "Edit" (Sửa))
                DistrictCode = a.DistrictCode,
                WardCode = a.WardCode,
                IsDefault = a.IsDefault
            }).ToList();

            return new ApiResponseDTO<List<AddressBookDto>> { IsSuccess = true, Data = dtos };
        }

        // 2. API: [POST] /api/addresses
        public async Task<ApiResponseDTO<AddressBookDto>> CreateAddressAsync(int customerId, CreateAddressBookRequestDto dto)
        {
            // 1. (Validate (Xác thực) Vị trí (Location)) & Lấy "Snapshot" (Sao chép) Tên (Name)
            var validationResult = await ValidateAndSnapshotLocation(dto.ProvinceCode, dto.DistrictCode, dto.WardCode);
            if (!validationResult.IsSuccess)
            {
                return new ApiResponseDTO<AddressBookDto> { IsSuccess = false, Code = 400, Message = validationResult.ErrorMessage };
            }

            AddressBook? oldDefault = null;
            if (dto.IsDefault)
            {
                oldDefault = await _addressRepo.GetDefaultByUserIdAsync(customerId);
            }

            // 2. Tạo (Create) Entity (Thực thể)
            var newAddress = new AddressBook
            {
                UserId = customerId,
                FullName = dto.FullName,
                Phone = dto.Phone,
                AddressLine = dto.AddressLine,
                IsDefault = dto.IsDefault,

                // (Gán 6 trường đã "Snapshot" (Sao chép) / Validate (Xác thực))
                ProvinceCode = dto.ProvinceCode,
                DistrictCode = dto.DistrictCode,
                WardCode = dto.WardCode,
                ProvinceName = validationResult.ProvinceName,
                DistrictName = validationResult.DistrictName,
                WardName = validationResult.WardName
            };

            // 3. (Transaction (Giao dịch)) (Nếu Set Mặc định (Default))
            if (oldDefault != null)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                oldDefault.IsDefault = false; // (Tắt Mặc định (Default) cũ)
                _addressRepo.Update(oldDefault);
                await _addressRepo.AddAsync(newAddress);
                await _addressRepo.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            else // (Transaction (Giao dịch) bình thường)
            {
                await _addressRepo.AddAsync(newAddress);
                await _addressRepo.SaveChangesAsync();
            }

            // 4. Trả về (Return) DTO (đã "Snapshot" (Sao chép))
            var resultDto = new AddressBookDto
            {
                Id = newAddress.Id,
                FullName = newAddress.FullName,
                Phone = newAddress.Phone,
                AddressLine = newAddress.AddressLine,
                ProvinceName = newAddress.ProvinceName,
                DistrictName = newAddress.DistrictName,
                WardName = newAddress.WardName,
                ProvinceCode = newAddress.ProvinceCode,
                DistrictCode = newAddress.DistrictCode,
                WardCode = newAddress.WardCode,
                IsDefault = newAddress.IsDefault
            };

            return new ApiResponseDTO<AddressBookDto> { IsSuccess = true, Data = resultDto };
        }

        // 3. API: [PUT] /api/addresses/{id}
        public async Task<ApiResponseDTO<AddressBookDto>> UpdateAddressAsync(int addressId, int customerId, UpdateAddressBookRequestDto dto)
        {
            // 1. (Check 404/403)
            var address = await _addressRepo.GetByIdAndUserIdAsync(addressId, customerId);
            if (address == null)
            {
                return new ApiResponseDTO<AddressBookDto> { IsSuccess = false, Code = 404, Message = "Address not found or permission denied." };
            }

            // 2. (Validate (Xác thực) Vị trí (Location) mới)
            var validationResult = await ValidateAndSnapshotLocation(dto.ProvinceCode, dto.DistrictCode, dto.WardCode);
            if (!validationResult.IsSuccess)
            {
                return new ApiResponseDTO<AddressBookDto> { IsSuccess = false, Code = 400, Message = validationResult.ErrorMessage };
            }

            // (Logic Transaction (Giao dịch) cho IsDefault (nếu cần, tương tự Create))
            // ...

            // 3. (Ghi đè (Overwrite))
            address.FullName = dto.FullName;
            address.Phone = dto.Phone;
            address.AddressLine = dto.AddressLine;
            address.IsDefault = dto.IsDefault;
            address.ProvinceCode = dto.ProvinceCode;
            address.DistrictCode = dto.DistrictCode;
            address.WardCode = dto.WardCode;
            address.ProvinceName = validationResult.ProvinceName;
            address.DistrictName = validationResult.DistrictName;
            address.WardName = validationResult.WardName;
            address.UpdatedAt = System.DateTime.UtcNow;

            _addressRepo.Update(address);
            await _addressRepo.SaveChangesAsync();

            // 4. (Trả về (Return) DTO (Tương tự Create))
            // (Tạm thời return OK, nên map (ánh xạ) lại DTO như Create)
            return new ApiResponseDTO<AddressBookDto> { IsSuccess = true, Message = "Updated" };
        }

        // 4. API: [DELETE] /api/addresses/{id}
        public async Task<ApiResponseDTO<string>> DeleteAddressAsync(int addressId, int customerId)
        {
            var address = await _addressRepo.GetByIdAndUserIdAsync(addressId, customerId);
            if (address == null)
            {
                return new ApiResponseDTO<string> { IsSuccess = false, Code = 404, Message = "Address not found or permission denied." };
            }

            // (Quy tắc Nghiệp vụ (Business Rule))
            if (address.IsDefault)
            {
                return new ApiResponseDTO<string> { IsSuccess = false, Code = 400, Message = "Cannot delete the default address." };
            }

            _addressRepo.Delete(address);
            await _addressRepo.SaveChangesAsync();

            return new ApiResponseDTO<string> { IsSuccess = true, Message = "Address deleted successfully." };
        }

        // 5. API: [PUT] /api/addresses/{id}/set-default
        public async Task<ApiResponseDTO<string>> SetDefaultAddressAsync(int addressId, int customerId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. (Check 404/403)
                var newDefault = await _addressRepo.GetByIdAndUserIdAsync(addressId, customerId);
                if (newDefault == null)
                {
                    await transaction.RollbackAsync();
                    return new ApiResponseDTO<string> { IsSuccess = false, Code = 404, Message = "Address not found or permission denied." };
                }

                // 2. (Tìm Mặc định (Default) cũ)
                var oldDefault = await _addressRepo.GetDefaultByUserIdAsync(customerId);
                if (oldDefault != null)
                {
                    oldDefault.IsDefault = false;
                    _addressRepo.Update(oldDefault);
                }

                // 3. (Set Mặc định (Default) mới)
                newDefault.IsDefault = true;
                _addressRepo.Update(newDefault);

                await _addressRepo.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ApiResponseDTO<string> { IsSuccess = true, Message = "Default address updated." };
            }
            catch (System.Exception ex)
            {
                await transaction.RollbackAsync();
                return new ApiResponseDTO<string> { IsSuccess = false, Code = 500, Message = $"Transaction failed: {ex.Message}" };
            }
        }

        /// <summary>
        /// (Hàm Helper (Trợ giúp)) Kiểm tra 3 Mã (Code) Vị trí (Location) và "Snapshot" (Sao chép) Tên (Name)
        /// </summary>
        private async Task<(bool IsSuccess, string ErrorMessage, string ProvinceName, string DistrictName, string WardName)>
            ValidateAndSnapshotLocation(string pCode, string dCode, string wCode)
        {
            var p = await _locationRepo.GetProvinceByCodeAsync(pCode);
            if (p == null) return (false, $"ProvinceCode '{pCode}' not found.", "", "", "");

            var d = await _locationRepo.GetDistrictByCodeAsync(dCode);
            if (d == null) return (false, $"DistrictCode '{dCode}' not found.", "", "", "");

            var w = await _locationRepo.GetWardByCodeAsync(wCode);
            if (w == null) return (false, $"WardCode '{wCode}' not found.", "", "", "");

            // (Tùy chọn: Check (Kiểm tra) Cha-Con (Parent-Child) (ví dụ: District phải thuộc Province))
            // ...

            return (true, "", p.Name, d.Name, w.Name);
        }
    }
}
