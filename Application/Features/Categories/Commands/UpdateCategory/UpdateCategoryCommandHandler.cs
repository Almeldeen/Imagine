using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Core.Interfaces;
using MediatR;

namespace Application.Features.Categories.Commands.UpdateCategory
{
    public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, BaseResponse<bool>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IImageService _imageService;

        public UpdateCategoryCommandHandler(ICategoryRepository categoryRepository, IImageService imageService)
        {
            _categoryRepository = categoryRepository;
            _imageService = imageService;
        }

        public async Task<BaseResponse<bool>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (category == null)
            {
                throw new NotFoundException("Category", request.Id);
            }

            category.Name = request.Name;
            category.Description = request.Description;
            category.IsActive = request.IsActive;
            category.DisplayOrder = request.DisplayOrder;

            // If a new image file is provided, replace the existing one via ImageService
            if (request.NewImageStream != null && !string.IsNullOrWhiteSpace(request.NewImageFileName))
            {
                var replace = await _imageService.ReplaceImageAsync(
                    request.NewImageStream,
                    request.NewImageFileName,
                    category.ImageUrl,
                    folder: "categories",
                    cancellationToken);

                if (!replace.Success)
                {
                    return BaseResponse<bool>.FailureResponse(replace.Message);
                }

                category.ImageUrl = replace.Data;
            }
            else if (request.ImagePath != null)
            {
                // Fallback/manual override for legacy callers
                category.ImageUrl = request.ImagePath;
            }

            category.UpdatedAt = DateTime.UtcNow;

            await _categoryRepository.UpdateAsync(category, cancellationToken);

            return BaseResponse<bool>.SuccessResponse(true, "Category updated successfully");
        }
    }
}
