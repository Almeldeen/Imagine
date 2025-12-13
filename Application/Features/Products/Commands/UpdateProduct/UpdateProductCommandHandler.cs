using Application.Common.Models;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Core.Interfaces;
using MediatR;

namespace Application.Features.Products.Commands.UpdateProduct
{
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, BaseResponse<bool>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IImageService _imageService;

        public UpdateProductCommandHandler(IProductRepository productRepository, IImageService imageService)
        {
            _productRepository = productRepository;
            _imageService = imageService;
        }

        public async Task<BaseResponse<bool>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
            if (product == null)
                throw new NotFoundException("Product", request.Id);

            product.Name = request.Name;
            product.Description = request.Description;
            product.BasePrice = request.Price;
            product.IsActive = request.IsActive;
            product.CategoryId = request.CategoryId;
            product.IsFeatured = request.IsFeatured;
            product.IsPopular = request.IsPopular;
            product.IsLatest = request.IsLatest;
            product.AllowAiCustomization = request.AllowAiCustomization;
            product.AvailableSizes = request.AvailableSizes;

            if (request.NewImageStream != null && !string.IsNullOrWhiteSpace(request.NewImageFileName))
            {
                var replace = await _imageService.ReplaceImageAsync(
                    request.NewImageStream,
                    request.NewImageFileName,
                    product.MainImageUrl,
                    folder: "products",
                    cancellationToken);

                if (!replace.Success)
                    return BaseResponse<bool>.FailureResponse(replace.Message);

                product.MainImageUrl = replace.Data;
            }

            product.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(product, cancellationToken);

            return BaseResponse<bool>.SuccessResponse(true, "Product updated successfully");
        }
    }
}
