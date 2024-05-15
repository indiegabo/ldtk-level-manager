using System.Collections.Generic;

namespace LDtkVania
{
    public class MV_PaginatedResponse<T>
    {
        public int TotalCount { get; set; }
        public List<T> Items { get; set; }
    }

    public struct MV_PaginationInfo
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
}