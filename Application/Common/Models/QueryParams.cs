using Application.Common.Enums;

namespace Application.Common.Models
{
    public class QueryParams : PaginationParams
    {
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; }
        public SortDirection SortDirection { get; set; } = SortDirection.Asc;
    }
}
