using Application.Common.Models;
using Application.Common.Exceptions;
using Application.Features.Products.DTOs;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Queries.GetProductById
{
    public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, BaseResponse<ProductDetailsDto>>
    {
        private readonly IProductRepository _productRepository;

        public GetProductByIdQueryHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<BaseResponse<ProductDetailsDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var query = _productRepository
                .GetAllQueryable()
                .AsNoTracking()
                .Where(p => p.Id == request.Id);

            var dto = await query
                .Select(p => new ProductDetailsDto
                {
                    Id = p.Id,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.BasePrice,
                    IsActive = p.IsActive,
                    IsFeatured = p.IsFeatured,
                    IsPopular = p.IsPopular,
                    IsLatest = p.IsLatest,
                    ViewCount = p.ViewCount,
                    AllowAiCustomization = p.AllowAiCustomization,
                    ImageUrl = p.MainImageUrl,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    AvailableSizes = p.AvailableSizes,
                    Colors = p.Colors
                        .Where(c => c.IsAvailable)
                        .Select(c => new ProductColorDto
                        {
                            Id = c.Id,
                            ProductId = c.ProductId,
                            ColorName = c.ColorName,
                            ColorHex = c.ColorHex,
                            Stock = c.Stock,
                            AdditionalPrice = c.AdditionalPrice,
                            IsAvailable = c.IsAvailable,
                            Images = c.Images
                                .OrderBy(i => i.DisplayOrder)
                                .Select(i => new ProductImageDto
                                {
                                    Id = i.Id,
                                    ProductColorId = i.ProductColorId,
                                    ImageUrl = i.ImageUrl,
                                    AltText = i.AltText,
                                    IsMain = i.IsMain,
                                    DisplayOrder = i.DisplayOrder
                                })
                                .ToList()
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (dto == null)
            {
                throw new NotFoundException("Product", request.Id);
            }

            return BaseResponse<ProductDetailsDto>.SuccessResponse(dto, "Product retrieved successfully");
        }
    }
}
