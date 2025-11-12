namespace BackendEcommerce.Application.Shared.DTOs
{
    public class PaginationRequestDto
    {
        private const int MaxPageSize = 50;
        private int _pageSize = 10; // Default page size

        public int PageNumber { get; set; } = 1; // Default page number

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }
    }
}
