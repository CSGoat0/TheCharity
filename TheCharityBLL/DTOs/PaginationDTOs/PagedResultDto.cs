
namespace TheCharityBLL.DTOs.PaginationDTOs
{
    public class PagedResultDto<T>
    {
        public IEnumerable<T> Items { get; set; } = [];
        /// <example>150</example>
        public int TotalCount { get; set; }
        /// <example>1</example>
        public int PageNumber { get; set; }
        /// <example>10</example>
        public int PageSize { get; set; }
        /// <example>15</example>
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        /// <example>true</example>
        public bool HasPrevious => PageNumber > 1;
        /// <example>true</example>
        public bool HasNext => PageNumber < TotalPages;
    }
}
