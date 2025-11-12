using System.ComponentModel.DataAnnotations;

namespace BackendEcommerce.Application.Features.SellerRegistration.DTOs
{
    public class SellerRegistrationDTOs
    {
        [Required]
        [MinLength(5)]
        [MaxLength(255)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        [RegularExpression(@"^\d{9,14}$", ErrorMessage = "Số tài khoản ngân hàng không hợp lệ.")]
        public string BankAccountNumber { get; set; }
    }
}