using Application.Common.Models;
using Application.Features.Products.DTOs;
using Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Queries.GetFeaturedProducts
{
    public class GetFeaturedProductsQueryHandler : IRequestHandler<GetFeaturedProductsQuery, BaseResponse<List<ProductListDto>>>
    {
        private readonly IProductRepository _productRepository;

        public GetFeaturedProductsQueryHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<BaseResponse<List<ProductListDto>>> Handle(GetFeaturedProductsQuery request, CancellationToken cancellationToken)
        {
            var query = _productRepository
                .GetAllQueryable()
                .AsNoTracking()
                .Include(p => p.Colors)
                    .ThenInclude(c => c.Images)
                .Where(p => p.IsActive && p.IsFeatured)
                .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
                .Take(request.Take);

            var items = await query
                .Select(p => new ProductListDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.BasePrice,
                    IsActive = p.IsActive,
                    IsFeatured = p.IsFeatured,
                    ViewCount = p.ViewCount,
                    ImageUrl = p.MainImageUrl,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
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
                .ToListAsync(cancellationToken);

            return BaseResponse<List<ProductListDto>>.SuccessResponse(items, "Featured products retrieved successfully");
        }
    }
}
