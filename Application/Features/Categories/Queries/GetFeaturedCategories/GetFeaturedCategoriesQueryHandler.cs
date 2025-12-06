using Application.Common.Models;
using Application.Features.Categories.DTOs;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Categories.Queries.GetFeaturedCategories
{
    public class GetFeaturedCategoriesQueryHandler : IRequestHandler<GetFeaturedCategoriesQuery, BaseResponse<List<CategoryDto>>>
    {
        private readonly ICategoryRepository _categoryRepository;

        public GetFeaturedCategoriesQueryHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<BaseResponse<List<CategoryDto>>> Handle(GetFeaturedCategoriesQuery request, CancellationToken cancellationToken)
        {
            var query = _categoryRepository
                .GetAllQueryable()
                .AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .Take(request.Take);

            var items = await query
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

            return BaseResponse<List<CategoryDto>>.SuccessResponse(items, "Featured categories retrieved successfully");
        }
    }
}
