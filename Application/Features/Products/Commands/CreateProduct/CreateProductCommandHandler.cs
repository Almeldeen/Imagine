using Application.Common.Models;
using Application.Common.Interfaces;
using Core.Entities;
using Core.Interfaces;
using MediatR;

namespace Application.Features.Products.Commands.CreateProduct
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, BaseResponse<int>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IImageService _imageService;

        public CreateProductCommandHandler(IProductRepository productRepository, IImageService imageService)
        {
            _productRepository = productRepository;
            _imageService = imageService;
        }

        public async Task<BaseResponse<int>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            string? imageUrl = null;

            // Optional: upload image if provided
            if (request.ImageStream != null && !string.IsNullOrWhiteSpace(request.ImageFileName))
            {
                var upload = await _imageService.UploadImageAsync(
                    request.ImageStream,
                    request.ImageFileName,
                    folder: "products",
                    cancellationToken);

                if (!upload.Success)
                {
                    return BaseResponse<int>.FailureResponse(upload.Message);
                }

                imageUrl = upload.Data;
            }

            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                BasePrice = request.Price,
                IsActive = request.IsActive,
                CategoryId = request.CategoryId,
                MainImageUrl = imageUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _productRepository.AddAsync(product, cancellationToken);

            return BaseResponse<int>.SuccessResponse(product.Id, "Product created successfully");
        }
    }
}
