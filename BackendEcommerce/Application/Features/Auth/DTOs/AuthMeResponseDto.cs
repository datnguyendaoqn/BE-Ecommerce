namespace BackendEcommerce.Application.Features.Auth.DTOs
{
    public class AuthMeResponseDto
    {
        // (Sau này có thể thêm thông tin User: Id, Email, FullName...)
        // public UserInfoDto User { get; set; }

        /// <summary>
        /// Tổng số lượng (Items Count) trong giỏ hàng (ví dụ: 5)
        /// Dùng để hiển thị "Badge" (Huy hiệu) trên Icon Giỏ hàng
        /// </summary>
        public int CartItemCount { get; set; }
    }
}
