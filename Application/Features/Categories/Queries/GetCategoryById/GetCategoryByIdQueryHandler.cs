using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Features.Categories.DTOs;
using Core.Entities;
using Core.Interfaces;
using MediatR;

namespace Application.Features.Categories.Queries.GetCategoryById
{
    public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, BaseResponse<CategoryDto>>
    {
        private readonly ICategoryRepository _categoryRepository;

        public GetCategoryByIdQueryHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<BaseResponse<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (category == null)
            {
                throw new NotFoundException("Category", request.Id);
            }

            var dto = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                IsActive = category.IsActive,
                DisplayOrder = category.DisplayOrder,
                ProductCount = category.Products.Count(p => p.IsActive),
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };

            return BaseResponse<CategoryDto>.SuccessResponse(dto, "Category retrieved successfully");
        }
    }
}
