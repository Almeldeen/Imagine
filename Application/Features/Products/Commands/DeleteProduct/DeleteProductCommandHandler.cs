using Application.Common.Models;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Core.Interfaces;
using MediatR;

namespace Application.Features.Products.Commands.DeleteProduct
{
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, BaseResponse<bool>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IImageService _imageService;

        public DeleteProductCommandHandler(IProductRepository productRepository, IImageService imageService)
        {
            _productRepository = productRepository;
            _imageService = imageService;
        }

        public async Task<BaseResponse<bool>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
            if (product == null)
                throw new NotFoundException("Product", request.Id);

            if (!string.IsNullOrWhiteSpace(product.MainImageUrl))
            {
                await _imageService.DeleteImageAsync(product.MainImageUrl, cancellationToken);
            }

            await _productRepository.DeleteAsync(product, cancellationToken);
            return BaseResponse<bool>.SuccessResponse(true, "Product deleted successfully");
        }
    }
}
