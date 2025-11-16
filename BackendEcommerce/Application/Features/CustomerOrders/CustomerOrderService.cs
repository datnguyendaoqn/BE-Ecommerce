using BackendEcommerce.Application.Features.CustomerOrders.Contracts;
using BackendEcommerce.Application.Features.CustomerOrders.DTOs;
using BackendEcommerce.Application.Features.Orders.Contracts;
using BackendEcommerce.Application.Shared.DTOs;
using BackendEcommerce.Infrastructure.Persistence.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Authentication;

namespace BackendEcommerce.Application.Features.CustomerOrders
{
    /// <summary>
    /// Triển khai các nghiệp vụ Quản lý Đơn hàng cho Customer.
    /// </summary>
    public class CustomerOrderService : ICustomerOrderService
    {
        private readonly IOrderRepository _orderRepo;
        // private readonly IMapper _mapper; // (Nếu dùng AutoMapper)

        // Chỉ cần OrderRepository
        public CustomerOrderService(IOrderRepository orderRepo)
        {
            _orderRepo = orderRepo;
        }

        /// <summary>
        /// Hàm GetMyOrdersAsync giờ trở nên rất gọn gàng
        /// </summary>
        public async Task<PagedListResponseDto<CustomerOrderResponseDto>> GetMyOrdersAsync(int userId, CustomerOrderFilterDto filter)
        {
            // 1. Gọi Repository (đã trả về DTO tối ưu)
            var (dtos, totalCount) = await _orderRepo.GetOrdersByUserIdAsync(
                userId,
                filter.Status,
                filter.PageNumber,
                filter.PageSize
            );
            var orders = dtos.ToList();
            // 2. Map sang DTO chung của PagedList (không cần map từng item nữa)
            return new PagedListResponseDto<CustomerOrderResponseDto>(
                orders,
                totalCount,
                filter.PageNumber,
                filter.PageSize
            );
        }

        /// <summary>
        /// (MỚI) Triển khai Lấy Chi tiết Đơn hàng
        /// </summary>
        public async Task<CustomerOrderDetailResponseDto> GetMyOrderDetailAsync(int userId, int orderId)
        {
            // 1. Lấy đơn hàng (CÓ tracking, vì có thể KHÔNG cần, nhưng GetByIdWithItemsAsync đang có)
            // Tái sử dụng hàm Repo đã có
            var order = await _orderRepo.GetOrderByIdWithItemsAsync(orderId);
            if (order == null)
            {
                throw new KeyNotFoundException("Không tìm thấy đơn hàng.");
            }

            // 2. KIỂM TRA BẢO MẬT: Đơn hàng này có thuộc Customer này không?
            if (order.UserId != userId)
            {
                throw new AuthenticationException("Bạn không có quyền xem đơn hàng này.");
            }

            // 3. Map sang DTO "Chi tiết" (DTO mới)
            return MapToCustomerDetailDto(order);
        }

        /// <summary>
        /// 2. Customer HỦY đơn (chỉ khi 'Pending')
        /// </summary>
        public async Task<CustomerOrderResponseDto> CancelMyPendingOrderAsync(int userId, int orderId, CustomerCancelOrderRequestDto dto)
        {
            // 1. Lấy đơn hàng (CÓ tracking, vì sẽ Update)
            var order = await _orderRepo.GetOrderByIdWithItemsAsync(orderId);
            if (order == null)
            {
                throw new KeyNotFoundException("Không tìm thấy đơn hàng.");
            }

            // 2. KIỂM TRA BẢO MẬT
            if (order.UserId != userId)
            {
                throw new AuthenticationException("Bạn không có quyền hủy đơn hàng này.");
            }

            // 3. LOGIC STATE MACHINE
            var currentStatus = order.Status.ToLower();
            if (currentStatus != "pending")
            {
                throw new ValidationException($"Bạn không thể hủy đơn hàng ở trạng thái '{currentStatus}'. Chỉ có thể hủy khi 'Pending'.");
            }

            // 4. Cập nhật và Lưu (theo mẫu UoW)
            order.Status = "cancelled"; // Trạng thái mới
            order.CancellationReason = dto.Reason; // <-- GÁN LÝ DO MỚI
            order.UpdatedAt = DateTime.UtcNow;

            _orderRepo.Update(order); // Đánh dấu
            await _orderRepo.SaveChangesAsync(); // Lưu

            // 5. TODO (Hoàn trả Tồn kho)
            // ...

            // 6. Map và trả về DTO
            return MapToCustomerDto(order);
        }

        /// <summary>
        /// 3. Customer XÁC NHẬN đã nhận hàng (chỉ khi 'Shipped')
        /// </summary>
        public async Task<CustomerOrderResponseDto> ConfirmMyDeliveryAsync(int userId, int orderId)
        {
            // 1. Lấy đơn hàng (CÓ tracking)
            var order = await _orderRepo.GetOrderByIdWithItemsAsync(orderId);
            if (order == null)
            {
                throw new KeyNotFoundException("Không tìm thấy đơn hàng.");
            }

            // 2. KIỂM TRA BẢO MẬT
            if (order.UserId != userId)
            {
                throw new AuthenticationException("Bạn không có quyền xác nhận đơn hàng này.");
            }

            // 3. LOGIC STATE MACHINE (Phía Customer)
            var currentStatus = order.Status.ToLower();
            if (currentStatus != "shipped")
            {
                throw new ValidationException($"Hành động không hợp lệ. Bạn chỉ có thể xác nhận khi đơn hàng ở trạng thái 'Shipped'.");
            }

            // 4. Cập nhật và Lưu (theo mẫu UoW)
            order.Status = "completed"; // Trạng thái mới
            order.UpdatedAt = DateTime.UtcNow;

            _orderRepo.Update(order);
            await _orderRepo.SaveChangesAsync();

            // 5. Map và trả về DTO
            return MapToCustomerDto(order);
        }

        // --- PRIVATE HELPER ---
        private CustomerOrderResponseDto MapToCustomerDto(Order order)
        {
            return new CustomerOrderResponseDto
            {
                Id = order.Id,
                ShopId = order.ShopId,
                ShopName = order.Shop.Name, // BẬT NẾU ĐÃ INCLUDE SHOP
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                PaymentMethod = order.PaymentMethod,
                CreatedAt = order.CreatedAt,
                TotalItemsCount = order.OrderItems.Count, // TỐI ƯU ĐẾM TẠI Ở ĐÂY
                Items = order.OrderItems.Select(i => new CustomerOrderItemDto
                {
                    ProductName = i.ProductName,
                    VariantName = i.VariantName,
                    Sku = i.Sku,
                    Quantity = i.Quantity,
                    Price = i.PriceAtTimeOfPurchase,
                    ImageUrl = i.ImageUrl
                }).ToList()
            };
        }

        /// <summary>
        /// (MỚI) Hàm helper để map sang DTO Chi tiết
        /// </summary>
        private CustomerOrderDetailResponseDto MapToCustomerDetailDto(Order order)
        {
            return new CustomerOrderDetailResponseDto
            {
                Id = order.Id,
                ShopId = order.ShopId,
                ShopName = order.Shop.Name, // BẬT NẾU ĐÃ INCLUDE SHOP
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                PaymentMethod = order.PaymentMethod,
                CreatedAt = order.CreatedAt,

                // Map chi tiết địa chỉ (Snapshot)
                ShippingName = order.Shipping_FullName,
                ShippingPhone = order.Shipping_Phone,
                ShippingAddressLine = order.Shipping_AddressLine,
                ShippingWard = order.Shipping_Ward,
                ShippingDistrict = order.Shipping_District,
                ShippingCity = order.Shipping_City,
                ShippingNote = order.Shipping_Note,
                CancellationReason = order.CancellationReason,
                // Map Items (dùng DTO con chung)
                Items = order.OrderItems.Select(i => new CustomerOrderItemDto
                {
                    ProductName = i.ProductName,
                    VariantName = i.VariantName,
                    Sku = i.Sku,
                    Quantity = i.Quantity,
                    Price = i.PriceAtTimeOfPurchase,
                    ImageUrl = i.ImageUrl
                }).ToList()
            };
        }
    }
}
