using Application.Common.Models;
using Application.Common.Enums;
using Application.Features.Products.DTOs;
using MediatR;

namespace Application.Features.Products.Queries.GetProductsList
{
    public class GetProductsListQuery : IRequest<BaseResponse<List<ProductListDto>>>
    {
        // Free text search over Name and Description
        public string? SearchTerm { get; set; }

        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Sorting: Name | Price | CreatedAt | UpdatedAt | ImageUrl | ViewCount
        public string? SortBy { get; set; }
        public SortDirection SortDirection { get; set; } = SortDirection.Asc;

        // Filters
        public int? CategoryId { get; set; }
        public string? ColorHex { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? IsActive { get; set; }
    }
}
