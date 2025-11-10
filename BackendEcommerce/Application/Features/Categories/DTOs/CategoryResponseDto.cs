namespace BackendEcommerce.Application.Features.Categories.DTOs
{
    public class RecursiveCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int? ParentId { get; set; }

        /// <summary>
        /// Danh sách các "con" (đã được xây dựng)
        /// </summary>
        public List<RecursiveCategoryDto> Children { get; set; } = new List<RecursiveCategoryDto>();
    }
}
