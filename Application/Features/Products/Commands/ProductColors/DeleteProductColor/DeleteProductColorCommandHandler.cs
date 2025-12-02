using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Commands.ProductColors.DeleteProductColor
{
    public class DeleteProductColorCommandHandler : IRequestHandler<DeleteProductColorCommand, BaseResponse<bool>>
    {
        private readonly IProductColorRepository _productColorRepository;
        private readonly IProductImageRepository _productImageRepository;
        private readonly IImageService _imageService;

        public DeleteProductColorCommandHandler(
            IProductColorRepository productColorRepository,
            IProductImageRepository productImageRepository,
            IImageService imageService)
        {
            _productColorRepository = productColorRepository;
            _productImageRepository = productImageRepository;
            _imageService = imageService;
        }

        public async Task<BaseResponse<bool>> Handle(DeleteProductColorCommand request, CancellationToken cancellationToken)
        {
            var color = await _productColorRepository.GetAllQueryable()
                .Include(c => c.Images)
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (color == null)
            {
                throw new NotFoundException("ProductColor", request.Id);
            }

            // Delete images from storage
            foreach (var image in color.Images)
            {
                if (!string.IsNullOrWhiteSpace(image.ImageUrl))
                {
                    await _imageService.DeleteImageAsync(image.ImageUrl, cancellationToken);
                }
            }

            await _productColorRepository.DeleteAsync(color, cancellationToken);

            return BaseResponse<bool>.SuccessResponse(true, "Product color deleted successfully");
        }
    }
}
