using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Core.Interfaces;
using MediatR;

namespace Application.Features.Categories.Commands.DeleteCategory
{
    public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, BaseResponse<bool>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IImageService _imageService;

        public DeleteCategoryCommandHandler(ICategoryRepository categoryRepository, IImageService imageService)
        {
            _categoryRepository = categoryRepository;
            _imageService = imageService;
        }

        public async Task<BaseResponse<bool>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (category == null)
            {
                throw new NotFoundException("Category", request.Id);
            }

            if (!string.IsNullOrWhiteSpace(category.ImageUrl))
            {
                await _imageService.DeleteImageAsync(category.ImageUrl, cancellationToken);
            }

            await _categoryRepository.DeleteAsync(category, cancellationToken);

            return BaseResponse<bool>.SuccessResponse(true, "Category deleted successfully");
        }
    }
}
