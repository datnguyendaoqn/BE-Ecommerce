using BackendEcommerce.Application.Shared.DTOs;

namespace BackendEcommerce.Application.Features.Orders.DTOs
{
    public class OrderFilterDto : PaginationRequestDto
    {
        public string? Status { get; set; } // Chỉ cần thêm cái riêng của nó
                                            // public DateTime? FromDate { get; set; } // Mở rộng sau này
    }
}
