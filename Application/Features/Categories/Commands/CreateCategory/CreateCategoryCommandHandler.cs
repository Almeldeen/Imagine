using Application.Common.Models;
using Core.Entities;
using Core.Interfaces;
using MediatR;

namespace Application.Features.Categories.Commands.CreateCategory
{
    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, BaseResponse<int>>
    {
        private readonly ICategoryRepository _categoryRepository;

        public CreateCategoryCommandHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<BaseResponse<int>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = new Category
            {
                Name = request.Name,
                Description = request.Description,
                ImageUrl = request.ImagePath,
                IsActive = request.IsActive,
                DisplayOrder = request.DisplayOrder,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _categoryRepository.AddAsync(category, cancellationToken);

            return BaseResponse<int>.SuccessResponse(category.Id, "Category created successfully");
        }
    }
}
