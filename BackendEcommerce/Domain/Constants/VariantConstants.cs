namespace BackendEcommerce.Domain.Constants
{
    public static class VariantConstants
    {
        public static readonly HashSet<string> AllowedSizes =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "XS", "S", "M", "L", "XL", "XXL", "XXXL",
                "Free Size", "One Size", "M/L", "L/XL",
                "28", "29", "30", "31", "32", "33", "34", "35",
                "36", "37", "38", "39", "40"
            };

        public static readonly HashSet<string> AllowedColors =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Đen", "Trắng", "Đỏ", "Xanh dương", "Xanh lá",
                "Vàng", "Hồng", "Tím", "Nâu", "Be",
                "Xám", "Cam", "Bạc", "Kem", "Rêu",
                "Xanh navy", "Xanh pastel", "Xanh ngọc",
                "Xanh mint", "Ghi", "Xanh than"
            };
    }
}
