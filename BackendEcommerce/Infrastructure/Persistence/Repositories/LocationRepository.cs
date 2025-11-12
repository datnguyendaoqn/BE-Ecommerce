using BackendEcommerce.Application.Features.Locations.Contracts;
using BackendEcommerce.Application.Features.Locations.DTOs;
using BackendEcommerce.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace BackendEcommerce.Infrastructure.Persistence.Repositories
{
    public class LocationRepository : ILocationRepository
    {
        private readonly EcomDbContext _context;

        public LocationRepository(EcomDbContext context)
        {
            _context = context;
        }

        public async Task<List<LocationDto>> GetProvincesAsync()
        {
            return await _context.Provinces
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .Select(p => new LocationDto
                {
                    Code = p.Code,
                    Name = p.Name
                })
                .ToListAsync();
        }

        public async Task<List<LocationDto>> GetDistrictsByProvinceCodeAsync(string provinceCode)
        {
            return await _context.Districts
                .AsNoTracking()
                .Where(d => d.ProvinceCode == provinceCode)
                .OrderBy(d => d.Name)
                .Select(d => new LocationDto
                {
                    Code = d.Code,
                    Name = d.Name
                })
                .ToListAsync();
        }

        public async Task<List<LocationDto>> GetWardsByDistrictCodeAsync(string districtCode)
        {
            return await _context.Wards
                .AsNoTracking()
                .Where(w => w.DistrictCode == districtCode)
                .OrderBy(w => w.Name)
                .Select(w => new LocationDto
                {
                    Code = w.Code,
                    Name = w.Name
                })
                .ToListAsync();
        }
        // === TRIỂN KHAI 3 HÀM TRA CỨU "SNAPSHOT" (SAO CHÉP) MỚI ===

        public async Task<LocationDto?> GetProvinceByCodeAsync(string provinceCode)
        {
            return await _context.Provinces
                .AsNoTracking()
                .Where(p => p.Code == provinceCode)
                .Select(p => new LocationDto { Code = p.Code, Name = p.Name })
                .FirstOrDefaultAsync();
        }

        public async Task<LocationDto?> GetDistrictByCodeAsync(string districtCode)
        {
            return await _context.Districts
                .AsNoTracking()
                .Where(d => d.Code == districtCode)
                .Select(d => new LocationDto { Code = d.Code, Name = d.Name })
                .FirstOrDefaultAsync();
        }

        public async Task<LocationDto?> GetWardByCodeAsync(string wardCode)
        {
            return await _context.Wards
                .AsNoTracking()
                .Where(w => w.Code == wardCode)
                .Select(w => new LocationDto { Code = w.Code, Name = w.Name })
                .FirstOrDefaultAsync();
        }
    }
}
