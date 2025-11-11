using BackendEcommerce.Application.Features.Carts.DTOs;
using BackendEcommerce.Application.Shared.DTOs;

namespace BackendEcommerce.Application.Features.Carts.Contracts
{
    public interface ICartService
    {
        /// <summary>
        /// (Nhanh - Chỉ đọc Redis) Lấy "Snapshot" (Sao chép) giỏ hàng.
        /// (Dùng nội bộ bởi AuthService)
        /// </summary>
        Task<CartSnapshotDto> GetCartSnapshotAsync(int customerId);

        /// <summary>
        /// (Chậm & Mới) Lấy giỏ hàng VÀ "Refresh" (Đồng bộ) với Oracle DB.
        /// (Dùng cho [GET] /api/cart - Trang Giỏ hàng đầy đủ)
        /// </summary>
        Task<ApiResponseDTO<CartSnapshotDto>> GetAndRefreshCartAsync(int customerId);

        /// <summary>
        /// (Nhanh - Check DB) Thêm/Cập nhật một món hàng.
        /// Trả về số lượng item MỚI.
        /// </summary>
        Task<ApiResponseDTO<int>> AddOrUpdateItemAsync(int customerId, AddCartItemRequestDto dto);

        /// <summary>
        /// (Nhanh) Xóa một món hàng.
        /// Trả về số lượng item MỚI.
        /// </summary>
        Task<ApiResponseDTO<int>> DeleteItemAsync(int customerId, int variantId);
        Task<ApiResponseDTO<int>> ClearCartAsync(int customerId);
    }
}
