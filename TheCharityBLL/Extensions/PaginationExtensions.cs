using TheCharityBLL.DTOs.PaginationDTOs;

namespace TheCharityBLL.Extensions
{
    public static class PaginationExtensions
    {
        public static PagedResultDto<TDestination> ToPagedResult<TSource, TDestination>(
        this (IEnumerable<TSource> Data, int TotalCount) result,
        IEnumerable<TDestination> mappedItems,
        PaginationParametersDto pagination)
        {
            return new PagedResultDto<TDestination>
            {
                Items = mappedItems,
                TotalCount = result.TotalCount,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize
            };
        }
    }
}
