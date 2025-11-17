using BackendEcommerce.Application.Features.Products.Contracts;
using BackendEcommerce.Application.Features.Products.DTOs;
using BackendEcommerce.Application.Shared.DTOs;
using BackendEcommerce.Infrastructure.Persistence.Data;
using BackendEcommerce.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendEcommerce.Infrastructure.Persistence.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly EcomDbContext _context;

        public ProductRepository(EcomDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<Product>> GetProductsByShopIdAsync(int shopId)
        {
            // Lấy tất cả sản phẩm (trừ các sản phẩm đã bị "xóa mềm")
            // Chỉ select các cột cần thiết cho "ProductSummaryDto" (Tối ưu)
            return await _context.Products
                // === THÊM .Include() VÀO ĐÂY ===
                .Include(p => p.Category) // Gộp bảng Category vào
                .Where(p => p.ShopId == shopId && p.Status != "deleted")
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<Product?> GetProductDetailByIdAsync(int productId)
        {
            // Đây là query "production-ready"
            // .Include(): Lấy Product (cha)
            // .ThenInclude(): Lấy Variant (con)
            // .Include(): Lấy Shop (để check quyền sở hữu)
            // .Include(): Lấy Category (để lấy tên)
            return await _context.Products
                .Include(p => p.Variants)
                .Include(p => p.Shop)
                .Include(p => p.Category)
                .Where(p => p.Status != "deleted") // Chỉ lấy sản phẩm còn hoạt động
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == productId);
        }
        public async Task<bool> ExistsAsync(int productId)
        {
            // Dùng AnyAsync là cách nhanh nhất để check tồn tại
            return await _context.Products
                .AnyAsync(p => p.Id == productId && p.Status != "deleted");
        }
        public void Update(Product product)
        {
            // Không cần "async", đây là hàm đồng bộ (synchronous)
            // Nó chỉ "báo" cho ChangeTracker của EF Core là "entity này đã bị sửa"
            _context.Products.Update(product);
        }
        public async Task<Product?> GetProductForUpdateAsync(int productId)
        {
            // Chỉ Include() những gì tối thiểu cần cho validation và response
            return await _context.Products
                .Include(p => p.Shop)       // Cần cho check quyền 403
                .Include(p => p.Category)   // Cần để lấy Category.Name
                .FirstOrDefaultAsync(p => p.Id == productId);
        }

        //
        //
        public void Delete(Product product)
        {
            _context.Products.Remove(product);
        }
        //
        //

        public async Task<PagedListResponseDto<ProductCardDto>> GetPaginatedProductCardsAsync(ProductListQueryRequestDto query)
        {
            // 1. Bắt đầu IQueryable
            // Chỉ lấy sản phẩm "active"
            IQueryable<Product> productsQuery = _context.Products
                                                    .AsNoTracking() // Tối ưu (Chỉ đọc)
                                                    .Where(p => p.Status == "active");

            // 2. Áp dụng Filter (Lọc)
            if (query.CategoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == query.CategoryId.Value);
            }
            if (query.MinPrice.HasValue)
            {
                // (Giả định Product có cột MinPrice)
                productsQuery = productsQuery.Where(p => p.MinPrice >= query.MinPrice.Value);
            }
            if (query.MaxPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.MinPrice <= query.MaxPrice.Value);
            }
            if (query.MinRating.HasValue)
            {
                // (Giả định Product có cột AverageRating)
                productsQuery = productsQuery.Where(p => p.AverageRating >= query.MinRating.Value);
            }
            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                productsQuery = productsQuery.Where(p => p.Name.ToLower().Contains(query.SearchTerm.ToLower()));
            }

            // 3. Áp dụng Sorting (Sắp xếp)
            // (Sắp xếp TRƯỚC khi Select, để tận dụng index DB)
            switch (query.SortBy?.ToLower())
            {
                case "price_asc":
                    productsQuery = productsQuery.OrderBy(p => p.MinPrice);
                    break;
                case "price_desc":
                    productsQuery = productsQuery.OrderByDescending(p => p.MinPrice);
                    break;
                case "newest":
                    productsQuery = productsQuery.OrderByDescending(p => p.CreatedAt);
                    break;
                case "popular":
                    // (Giả định "popular" = "selledcount")
                    productsQuery = productsQuery.OrderByDescending(p => p.SelledCount);
                    break;
                default:
                    // Mặc định: Sắp xếp theo mức độ phổ biến
                    productsQuery = productsQuery.OrderByDescending(p => p.SelledCount);
                    break;
            }

            // 4. Lấy TotalCount (TRƯỚC khi Phân trang)
            var totalCount = await productsQuery.CountAsync();

            // 5. Áp dụng Phân trang (Pagination)
            var pagedQuery = productsQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize);

            // 6. Ánh xạ (Select) sang DTO (SAU CÙNG)
            // (Chúng ta cần 3 cột "trade-off" và ảnh chính)
            var productCards = await pagedQuery
                .Select(p => new ProductCardDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    MinPrice = p.MinPrice,
                    AverageRating = p.AverageRating,
                    ReviewCount = p.ReviewCount,
                    SelledCount = p.SelledCount,

                    // Lấy ảnh chính (Giả định: dùng sub-query)
                    // CẢNH BÁO: Sub-query này có thể chậm.
                    // Cách tốt hơn sau này là Join hoặc dùng Function của DB.
                    PrimaryImageUrl = _context.Media
                                        .Where(m => m.EntityId == p.Id &&
                                                    m.EntityType == "product" &&
                                                    m.IsPrimary == true)
                                        .Select(m => m.ImageUrl)
                                        .FirstOrDefault() ?? "https://placehold.co/300x300?text=No+Image" // Ảnh mặc định
                })
                .ToListAsync();

            // 7. Trả về DTO Phân trang (đã thống nhất)
            return new PagedListResponseDto<ProductCardDto>(
                productCards,
                totalCount,
                query.PageNumber,
                query.PageSize
            );
        }
        //
        //
        public async Task<Product?> GetProductEntityByIdAsync(int productId)
        {
            // Hàm đơn giản để lấy Entity (không tracking)
            return await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == productId);
        }
        public async Task<List<ProductCardDto>> GetRelatedProductsAsCardsAsync(
       int shopId,
       int currentProductId,
       int limit)
        {
            var query = _context.Products.AsNoTracking();

           
                query = query.Where(p => p.ShopId == shopId);
            // Loại trừ chính nó và Sắp xếp (luôn theo SelledCount)
            query = query.Where(p => p.Id != currentProductId);

            // Áp dụng Select (Projection)
            // (TÁI SỬ DỤNG logic map sang ProductCardDto từ hàm GetPaginatedProductCardsAsync)
            return await query
                .Take(limit)
                .Select(p => new ProductCardDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    PrimaryImageUrl = _context.Media.FirstOrDefault(m=>m.EntityId==p.Id).ImageUrl, // Giả sử tên cột là vậy
                    MinPrice = p.MinPrice, // Giả sử tên cột là vậy
                    AverageRating = p.AverageRating,
                    ReviewCount = p.ReviewCount,
                    SelledCount = p.SelledCount
                })
                .ToListAsync();
        }
    }
}
