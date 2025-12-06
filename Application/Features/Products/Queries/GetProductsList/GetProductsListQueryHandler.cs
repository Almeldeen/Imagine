using Application.Common.Models;
using Application.Common.Interfaces;
using Application.Features.Products.DTOs;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Queries.GetProductsList
{
    public class GetProductsListQueryHandler : IRequestHandler<GetProductsListQuery, BaseResponse<List<ProductListDto>>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IQueryService _queryService;

        public GetProductsListQueryHandler(IProductRepository productRepository, IQueryService queryService)
        {
            _productRepository = productRepository;
            _queryService = queryService;
        }

        public async Task<BaseResponse<List<ProductListDto>>> Handle(GetProductsListQuery request, CancellationToken cancellationToken)
        {
            // Start with a plain IQueryable<Product>. We will rely on the projection
            // below to generate the necessary joins for colors and images.
            var query = _productRepository
                .GetAllQueryable()
                .AsNoTracking();

            // Search by name or description if provided
            query = _queryService.ApplySearch<Product>(query, request.SearchTerm, nameof(Product.Name), nameof(Product.Description));

            // Filters
            if (request.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == request.CategoryId.Value);
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(p => p.IsActive == request.IsActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.ColorHex))
            {
                var colorHex = request.ColorHex.Trim();
                query = query.Where(p => p.Colors.Any(c => c.IsAvailable && c.ColorHex == colorHex));
            }

            if (request.MinPrice.HasValue)
            {
                query = query.Where(p => p.BasePrice >= request.MinPrice.Value);
            }

            if (request.MaxPrice.HasValue)
            {
                query = query.Where(p => p.BasePrice <= request.MaxPrice.Value);
            }

            // Count before pagination
            var totalItems = await query.CountAsync(cancellationToken);

            // Map friendly sort names to domain properties
            var sortBy = request.SortBy;
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (string.Equals(sortBy, "Price", StringComparison.OrdinalIgnoreCase))
                    sortBy = nameof(Product.BasePrice);
                else if (string.Equals(sortBy, "ImageUrl", StringComparison.OrdinalIgnoreCase))
                    sortBy = nameof(Product.MainImageUrl);
            }

            if (!string.IsNullOrWhiteSpace(sortBy))
                query = _queryService.ApplySorting(query, sortBy!, request.SortDirection);
            else
                query = query.OrderBy(p => p.Name);

            // Apply pagination
            query = _queryService.ApplyPagination(query, request.PageNumber, request.PageSize);

            // Project to DTO (colors and images are projected directly via navigation properties)
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

            return BaseResponse<List<ProductListDto>>.SuccessResponse(items, request.PageNumber, request.PageSize, totalItems, "Products retrieved successfully");
        }
    }
}
