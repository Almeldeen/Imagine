using Application.Common.Interfaces;
using Application.Common.Models;
using Core.Entities;
using Core.Interfaces;
using MediatR;

namespace Application.Features.Categories.Commands.CreateCategory
{
    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, BaseResponse<int>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IImageService _imageService;

        public CreateCategoryCommandHandler(ICategoryRepository categoryRepository, IImageService imageService)
        {
            _categoryRepository = categoryRepository;
            _imageService = imageService;
        }

        public async Task<BaseResponse<int>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            string? imageUrl = request.ImagePath;

            // If a file stream is provided, upload it via ImageService and use the generated URL
            if (request.ImageStream != null && !string.IsNullOrWhiteSpace(request.ImageFileName))
            {
                var upload = await _imageService.UploadImageAsync(
                    request.ImageStream,
                    request.ImageFileName,
                    folder: "categories",
                    cancellationToken);

                if (!upload.Success)
                {
                    return BaseResponse<int>.FailureResponse(upload.Message);
                }

                imageUrl = upload.Data;
            }

            var category = new Category
            {
                Name = request.Name,
                Description = request.Description,
                ImageUrl = imageUrl,
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
