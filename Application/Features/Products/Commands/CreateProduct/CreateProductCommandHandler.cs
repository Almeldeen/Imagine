using Application.Common.Interfaces;
using Application.Common.Models;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System;
using System.Linq;

namespace Application.Features.Products.Commands.CreateProduct
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, BaseResponse<int>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductColorRepository _productColorRepository;
        private readonly IProductImageRepository _productImageRepository;
        private readonly IImageService _imageService;
        private readonly IUnitOfWork _unitOfWork;

        public CreateProductCommandHandler(
            IProductRepository productRepository,
            IProductColorRepository productColorRepository,
            IProductImageRepository productImageRepository,
            IImageService imageService,
            IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _productColorRepository = productColorRepository;
            _productImageRepository = productImageRepository;
            _imageService = imageService;
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<int>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            try
            {
                BaseResponse<int>? result = null;

                await _unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    // 1) Upload main image if provided
                    string? mainImageUrl = null;

                    if (request.ImageStream != null && !string.IsNullOrWhiteSpace(request.ImageFileName))
                    {
                        var upload = await _imageService.UploadImageAsync(
                            request.ImageStream,
                            request.ImageFileName,
                            folder: "products",
                            cancellationToken: ct);

                        if (!upload.Success)
                        {
                            throw new InvalidOperationException(upload.Message);
                        }

                        mainImageUrl = upload.Data;
                    }

                    // 2) Create product
                    var product = new Product
                    {
                        CategoryId = request.CategoryId,
                        Name = request.Name,
                        Description = request.Description,
                        BasePrice = request.Price,
                        MainImageUrl = mainImageUrl,
                        IsActive = request.IsActive,
                        IsFeatured = request.IsFeatured,
                        IsPopular = request.IsPopular,
                        IsLatest = request.IsLatest,
                        AllowAiCustomization = request.AllowAiCustomization,
                        AvailableSizes = request.AvailableSizes,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _productRepository.AddAsync(product, ct);

                    // 3) Create colors + images (if any)
                    if (request.Colors != null && request.Colors.Count > 0)
                    {
                        foreach (var colorDto in request.Colors)
                        {
                            var color = new ProductColor
                            {
                                ProductId = product.Id,
                                ColorName = colorDto.ColorName,
                                ColorHex = colorDto.ColorHex,
                                Stock = colorDto.Stock,
                                AdditionalPrice = colorDto.AdditionalPrice,
                                IsAvailable = colorDto.IsAvailable,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };

                            await _productColorRepository.AddAsync(color, ct);

                            if (colorDto.Images == null || colorDto.Images.Count == 0)
                            {
                                continue;
                            }

                            var orderedImages = colorDto.Images
                                .OrderBy(i => i.DisplayOrder)
                                .ToList();

                            bool anyMain = orderedImages.Any(i => i.IsMain);
                            int index = 0;

                            foreach (var imageDto in orderedImages)
                            {
                                if (string.IsNullOrWhiteSpace(imageDto.FileKey))
                                {
                                    index++;
                                    continue;
                                }

                                if (!request.ImageStreams.TryGetValue(imageDto.FileKey, out var imageStream) ||
                                    !request.ImageFileNames.TryGetValue(imageDto.FileKey, out var imageFileName))
                                {
                                    index++;
                                    continue;
                                }

                                var upload = await _imageService.UploadImageAsync(
                                    imageStream,
                                    imageFileName,
                                    folder: "product-images",
                                    cancellationToken: ct);

                                if (!upload.Success)
                                {
                                    throw new InvalidOperationException(upload.Message);
                                }

                                bool isMain = imageDto.IsMain;
                                if (!anyMain && index == 0)
                                {
                                    isMain = true;
                                }

                                var image = new ProductImage
                                {
                                    ProductColorId = color.Id,
                                    ImageUrl = upload.Data!,
                                    AltText = imageDto.AltText,
                                    DisplayOrder = imageDto.DisplayOrder,
                                    IsMain = isMain,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                };

                                await _productImageRepository.AddAsync(image, ct);

                                index++;
                            }
                        }
                    }

                    result = BaseResponse<int>.SuccessResponse(product.Id, "Product created successfully with colors and images");
                }, cancellationToken);

                return result ?? BaseResponse<int>.FailureResponse("Failed to create product");
            }
            catch (Exception ex)
            {
                return BaseResponse<int>.FailureResponse($"Failed to create product: {ex.Message}");
            }
            finally
            {
                // Dispose uploaded streams
                if (request.ImageStream != null)
                {
                    request.ImageStream.Dispose();
                }

                foreach (var stream in request.ImageStreams.Values)
                {
                    stream?.Dispose();
                }
            }
        }
    }
}
