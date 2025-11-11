using Application.Common.Enums;
using Application.Common.Models;
using Application.Features.Categories.DTOs;
using MediatR;

namespace Application.Features.Categories.Queries.GetCategoriesList
{
    public class GetCategoriesListQuery : IRequest<BaseResponse<List<CategoryDto>>>
    {
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public SortDirection SortDirection { get; set; } = SortDirection.Asc;
    }
}
