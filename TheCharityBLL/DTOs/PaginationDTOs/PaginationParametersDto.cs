
namespace TheCharityBLL.DTOs.PaginationDTOs
{
    public class PaginationParametersDto
    {

        private const int MaxPageSize = 50;

        private int _pageSize = 10;
        /// <example>1</example>
        public int PageNumber { get; set; } = 1;
        /// <example>10</example>
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }
    }
}
