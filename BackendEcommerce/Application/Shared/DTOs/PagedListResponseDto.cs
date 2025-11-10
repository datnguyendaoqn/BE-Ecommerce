namespace BackendEcommerce.Application.Shared.DTOs
{
    public class PagedListResponseDto<T> where T : class
    {
        public List<T> Items { get; set; } = new List<T>();

        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }

        

        public PagedListResponseDto(List<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;
            PageNumber = pageNumber;
            TotalPages = (int)System.Math.Ceiling(count / (double)pageSize);
            Items = items;
        }
    }
}
