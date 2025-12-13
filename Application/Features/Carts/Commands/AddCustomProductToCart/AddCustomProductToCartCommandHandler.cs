using Application.Common.Models;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Carts.Commands.AddCustomProductToCart
{
    public class AddCustomProductToCartCommandHandler : IRequestHandler<AddCustomProductToCartCommand, BaseResponse<bool>>
    {
        private readonly ICartRepository _cartRepo;
        private readonly ICustomProductRepository _customProductRepo;

        public AddCustomProductToCartCommandHandler(ICartRepository cartRepo, ICustomProductRepository customProductRepo)
        {
            _cartRepo = cartRepo;
            _customProductRepo = customProductRepo;
        }

        public async Task<BaseResponse<bool>> Handle(AddCustomProductToCartCommand request, CancellationToken cancellationToken)
        {
            if (request.CustomProductId <= 0)
            {
                return BaseResponse<bool>.FailureResponse("A valid custom product id is required.");
            }

            if (request.Quantity <= 0)
            {
                return BaseResponse<bool>.FailureResponse("Quantity must be greater than zero.");
            }

            var customProduct = await _customProductRepo.GetByIdAsync(request.CustomProductId, cancellationToken);
            if (customProduct == null)
            {
                return BaseResponse<bool>.FailureResponse("Custom product not found.");
            }

            var cart = await _cartRepo.GetCartWithItemsAsync(request.UserOrSessionId);

            if (cart == null)
            {
                cart = new Cart
                {
                    SessionId = request.UserOrSessionId,
                    CreatedAt = DateTime.UtcNow
                };

                await _cartRepo.AddAsync(cart, cancellationToken);
                await _cartRepo.SaveChangeAsync(cancellationToken);
            }

            var existing = cart.Items.FirstOrDefault(i =>
                i.CustomProductId == request.CustomProductId &&
                i.Size == request.Size);

            var unitPrice = customProduct.EstimatedPrice > 0 ? customProduct.EstimatedPrice : 49.99m;

            if (existing != null)
            {
                existing.Quantity += request.Quantity;
                existing.UnitPrice = unitPrice;
                existing.TotalPrice = existing.UnitPrice * existing.Quantity;
            }
            else
            {
                var newItem = new CartItem
                {
                    CustomProductId = request.CustomProductId,
                    Quantity = request.Quantity,
                    Size = request.Size,
                    UnitPrice = unitPrice,
                    TotalPrice = unitPrice * request.Quantity,
                    CartId = cart.Id
                };

                await _cartRepo.AddCartItemAsync(newItem, cancellationToken);
            }

            await _cartRepo.SaveChangeAsync(cancellationToken);

            return BaseResponse<bool>.SuccessResponse(true, "Custom design added to cart.");
        }
    }
}
