using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Commands.ProductImages.AddProductImage
{
    public class AddProductImageCommandHandler : IRequestHandler<AddProductImageCommand, BaseResponse<int>>
    {
        private readonly IProductColorRepository _productColorRepository;
        private readonly IProductImageRepository _productImageRepository;
        private readonly IImageService _imageService;

        public AddProductImageCommandHandler(
            IProductColorRepository productColorRepository,
            IProductImageRepository productImageRepository,
            IImageService imageService)
        {
            _productColorRepository = productColorRepository;
            _productImageRepository = productImageRepository;
            _imageService = imageService;
        }

        public async Task<BaseResponse<int>> Handle(AddProductImageCommand request, CancellationToken cancellationToken)
        {
            var color = await _productColorRepository.GetAllQueryable()
                .Include(c => c.Images)
                .FirstOrDefaultAsync(c => c.Id == request.ProductColorId, cancellationToken);

            if (color == null)
            {
                throw new NotFoundException("ProductColor", request.ProductColorId);
            }

            if (request.ImageStream == null || string.IsNullOrWhiteSpace(request.ImageFileName))
            {
                return BaseResponse<int>.FailureResponse("Image file is required");
            }

            var upload = await _imageService.UploadImageAsync(
                request.ImageStream,
                request.ImageFileName,
                folder: "product-images",
                cancellationToken);

            if (!upload.Success)
            {
                return BaseResponse<int>.FailureResponse(upload.Message);
            }

            var nextOrder = color.Images.Any() ? color.Images.Max(i => i.DisplayOrder) + 1 : 0;
            var isMain = request.IsMain ?? !color.Images.Any();

            var image = new ProductImage
            {
                ProductColorId = request.ProductColorId,
                ImageUrl = upload.Data!,
                AltText = request.AltText,
                DisplayOrder = nextOrder,
                IsMain = isMain,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            if (isMain)
            {
                foreach (var existing in color.Images)
                {
                    existing.IsMain = false;
                    existing.UpdatedAt = DateTime.UtcNow;
                    await _productImageRepository.UpdateAsync(existing, cancellationToken);
                }
            }

            await _productImageRepository.AddAsync(image, cancellationToken);

            return BaseResponse<int>.SuccessResponse(image.Id, "Product image added successfully");
        }
    }
}
