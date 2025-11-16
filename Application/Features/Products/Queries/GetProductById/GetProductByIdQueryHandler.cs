using Application.Common.Models;
using Application.Common.Exceptions;
using Application.Features.Products.DTOs;
using Core.Interfaces;
using MediatR;

namespace Application.Features.Products.Queries.GetProductById
{
    public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, BaseResponse<ProductDto>>
    {
        private readonly IProductRepository _productRepository;

        public GetProductByIdQueryHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<BaseResponse<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
            if (product == null)
                throw new NotFoundException("Product", request.Id);

            var dto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.BasePrice,
                IsActive = product.IsActive,
                ImageUrl = product.MainImageUrl,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };

            return BaseResponse<ProductDto>.SuccessResponse(dto, "Product retrieved successfully");
        }
    }
}
