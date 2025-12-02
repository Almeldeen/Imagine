using Application.Common.Exceptions;
using Application.Common.Models;
using Core.Entities;
using Core.Interfaces;
using MediatR;

namespace Application.Features.Products.Commands.ProductColors.AddProductColor
{
    public class AddProductColorCommandHandler : IRequestHandler<AddProductColorCommand, BaseResponse<int>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductColorRepository _productColorRepository;

        public AddProductColorCommandHandler(IProductRepository productRepository, IProductColorRepository productColorRepository)
        {
            _productRepository = productRepository;
            _productColorRepository = productColorRepository;
        }

        public async Task<BaseResponse<int>> Handle(AddProductColorCommand request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product == null)
            {
                throw new NotFoundException("Product", request.ProductId);
            }

            var color = new ProductColor
            {
                ProductId = request.ProductId,
                ColorName = request.ColorName,
                ColorHex = request.ColorHex,
                Stock = request.Stock,
                AdditionalPrice = request.AdditionalPrice,
                IsAvailable = request.IsAvailable,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _productColorRepository.AddAsync(color, cancellationToken);

            return BaseResponse<int>.SuccessResponse(color.Id, "Product color added successfully");
        }
    }
}
