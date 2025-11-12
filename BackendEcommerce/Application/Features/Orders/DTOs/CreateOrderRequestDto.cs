using System.ComponentModel.DataAnnotations;

namespace BackendEcommerce.Application.Features.Orders.DTOs
{
    /// <summary>
    /// DTO Đầu vào (Input) cho [POST] /api/orders
    /// (ĐÃ VIẾT LẠI (REWRITTEN): Để hỗ trợ (support) "Tick" (Chọn) Item (Món hàng)
    /// và Ghi chú (Note) (Note) riêng (separate) cho Cửa hàng (Shop) (Shop))
    /// </summary>
    public class CreateOrderRequestDto
    {
        /// <summary>
        /// ID (Mã) của Địa chỉ (Address) (Address) đã chọn (select)
        /// </summary>
        [Required]
        public int AddressBookId { get; set; }

        /// <summary>
        /// Phương thức Thanh toán (Payment Method) (Payment Method) (PM) (Phương thức) (MVP (Sản phẩm Tối thiểu))
        /// </summary>
        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = "COD";

        /// <summary>
        /// [THAY ĐỔI] Danh sách (List) các Ghi chú (Note) (Note) riêng (separate) cho Cửa hàng (Shop) (Shop)
        /// (Giải quyết "Lỗ hổng 2")
        /// </summary>
        public List<ShopNoteDto>? ShopNotes { get; set; }

        /// <summary>
        /// [THAY ĐỔI] Danh sách (List) ID (Mã) Variant (Biến thể) (Biến thể) (Biến thể) (Biến thể) mà User (Người dùng) (Người dùng) đã "Tick" (Chọn)
        /// (Giải quyết "Lỗ hổng 1")
        /// </summary>
        [Required(ErrorMessage = "You must select at least one item to checkout.")]
        [MinLength(1, ErrorMessage = "You must select at least one item to checkout.")]
        public List<int> TickedVariantIds { get; set; } = new List<int>();
    }
}
