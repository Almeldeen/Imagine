using Application.Common.Models;
using Application.Common.Interfaces;
using Application.Features.Products.DTOs;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Queries.GetProductsList
{
    public class GetProductsListQueryHandler : IRequestHandler<GetProductsListQuery, BaseResponse<List<ProductDto>>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IQueryService _queryService;

        public GetProductsListQueryHandler(IProductRepository productRepository, IQueryService queryService)
        {
            _productRepository = productRepository;
            _queryService = queryService;
        }

        public async Task<BaseResponse<List<ProductDto>>> Handle(GetProductsListQuery request, CancellationToken cancellationToken)
        {
            var query = _productRepository.GetAllQueryable().AsNoTracking();

            // Search by name or description if provided
            query = _queryService.ApplySearch<Product>(query, request.SearchTerm, nameof(Product.Name), nameof(Product.Description));

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

            // Project to DTO
            var items = await query
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.BasePrice,
                    IsActive = p.IsActive,
                    ImageUrl = p.MainImageUrl,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .ToListAsync(cancellationToken);

            return BaseResponse<List<ProductDto>>.SuccessResponse(items, request.PageNumber, request.PageSize, totalItems, "Products retrieved successfully");
        }
    }
}
