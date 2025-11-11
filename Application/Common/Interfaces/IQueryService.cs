using Application.Common.Enums;

namespace Application.Common.Interfaces
{
    public interface IQueryService
    {
        IQueryable<T> ApplyPagination<T>(IQueryable<T> query, int pageNumber, int pageSize);
        IQueryable<T> ApplySorting<T>(IQueryable<T> query, string? sortBy, SortDirection sortDirection);
        IQueryable<T> ApplySearch<T>(IQueryable<T> query, string? searchTerm, params string[] searchProperties);
    }
}
