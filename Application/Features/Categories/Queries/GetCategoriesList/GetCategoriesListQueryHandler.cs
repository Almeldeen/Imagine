using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Categories.DTOs;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Categories.Queries.GetCategoriesList
{
    public class GetCategoriesListQueryHandler : IRequestHandler<GetCategoriesListQuery, BaseResponse<List<CategoryDto>>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IQueryService _queryService;

        public GetCategoriesListQueryHandler(ICategoryRepository categoryRepository, IQueryService queryService)
        {
            _categoryRepository = categoryRepository;
            _queryService = queryService;
        }

        public async Task<BaseResponse<List<CategoryDto>>> Handle(GetCategoriesListQuery request, CancellationToken cancellationToken)
        {
            var query = _categoryRepository.GetAllQueryable().AsNoTracking();

            if (request.IsActive.HasValue)
            {
                query = query.Where(c => c.IsActive == request.IsActive.Value);
            }

            query = _queryService.ApplySearch<Category>(query, request.SearchTerm, nameof(Category.Name), nameof(Category.Description));

            var totalItems = await query.CountAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(request.SortBy))
            {
                query = query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name);
            }
            else
            {
                query = _queryService.ApplySorting(query, request.SortBy, request.SortDirection);
            }

            query = _queryService.ApplyPagination(query, request.PageNumber, request.PageSize);

            var categories = await query
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ImageUrl = c.ImageUrl,
                    IsActive = c.IsActive,
                    DisplayOrder = c.DisplayOrder,
                    ProductCount = c.Products.Count(p => p.IsActive),
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .ToListAsync(cancellationToken);

            return BaseResponse<List<CategoryDto>>.SuccessResponse(
                categories,
                request.PageNumber,
                request.PageSize,
                totalItems,
                "Categories retrieved successfully"
            );
        }
    }
}
