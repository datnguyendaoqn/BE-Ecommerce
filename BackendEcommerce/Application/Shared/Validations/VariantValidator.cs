using BackendEcommerce.Domain.Constants;

namespace BackendEcommerce.Application.Shared.Validations
{
    public static class VariantValidator
    {
        public static string? ValidateAttributes(string? size, string? color)
        {
            if (!string.IsNullOrEmpty(size) && !VariantConstants.AllowedSizes.Contains(size))
                return $"Size '{size}' không hợp lệ. Giá trị hợp lệ: {string.Join(", ", VariantConstants.AllowedSizes)}.";

            if (!string.IsNullOrEmpty(color) && !VariantConstants.AllowedColors.Contains(color))
                return $"Màu '{color}' không hợp lệ. Ví dụ: {string.Join(", ", VariantConstants.AllowedColors.Take(5))}...";

            return null;
        }
    }
}
