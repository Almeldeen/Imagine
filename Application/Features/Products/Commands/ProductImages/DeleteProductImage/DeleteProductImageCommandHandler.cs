using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Core.Interfaces;
using MediatR;

namespace Application.Features.Products.Commands.ProductImages.DeleteProductImage
{
    public class DeleteProductImageCommandHandler : IRequestHandler<DeleteProductImageCommand, BaseResponse<bool>>
    {
        private readonly IProductImageRepository _productImageRepository;
        private readonly IImageService _imageService;

        public DeleteProductImageCommandHandler(IProductImageRepository productImageRepository, IImageService imageService)
        {
            _productImageRepository = productImageRepository;
            _imageService = imageService;
        }

        public async Task<BaseResponse<bool>> Handle(DeleteProductImageCommand request, CancellationToken cancellationToken)
        {
            var image = await _productImageRepository.GetByIdAsync(request.Id, cancellationToken);
            if (image == null)
            {
                throw new NotFoundException("ProductImage", request.Id);
            }

            if (!string.IsNullOrWhiteSpace(image.ImageUrl))
            {
                await _imageService.DeleteImageAsync(image.ImageUrl, cancellationToken);
            }

            await _productImageRepository.DeleteAsync(image, cancellationToken);

            return BaseResponse<bool>.SuccessResponse(true, "Product image deleted successfully");
        }
    }
}
