using Application.Common.Exceptions;
using Application.Common.Models;
using Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Commands.ProductImages.ReorderProductImages
{
    public class ReorderProductImagesCommandHandler : IRequestHandler<ReorderProductImagesCommand, BaseResponse<bool>>
    {
        private readonly IProductColorRepository _productColorRepository;
        private readonly IProductImageRepository _productImageRepository;

        public ReorderProductImagesCommandHandler(
            IProductColorRepository productColorRepository,
            IProductImageRepository productImageRepository)
        {
            _productColorRepository = productColorRepository;
            _productImageRepository = productImageRepository;
        }

        public async Task<BaseResponse<bool>> Handle(ReorderProductImagesCommand request, CancellationToken cancellationToken)
        {
            var color = await _productColorRepository.GetAllQueryable()
                .Include(c => c.Images)
                .FirstOrDefaultAsync(c => c.Id == request.ProductColorId, cancellationToken);

            if (color == null)
            {
                throw new NotFoundException("ProductColor", request.ProductColorId);
            }

            var images = color.Images.ToList();

            if (request.ImageIdsInOrder.Count != images.Count ||
                !request.ImageIdsInOrder.All(id => images.Any(i => i.Id == id)))
            {
                return BaseResponse<bool>.FailureResponse("ImageIdsInOrder does not match existing images set");
            }

            for (var index = 0; index < request.ImageIdsInOrder.Count; index++)
            {
                var imageId = request.ImageIdsInOrder[index];
                var image = images.First(i => i.Id == imageId);
                image.DisplayOrder = index;
                image.UpdatedAt = DateTime.UtcNow;
                await _productImageRepository.UpdateAsync(image, cancellationToken);
            }

            return BaseResponse<bool>.SuccessResponse(true, "Product images reordered successfully");
        }
    }
}
