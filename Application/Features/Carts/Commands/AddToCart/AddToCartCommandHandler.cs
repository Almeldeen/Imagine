using Application.Common.Models;
using Application.Features.Carts.DTOs;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;

namespace Application.Features.Carts.Commands.AddToCart
{
    public class AddToCartHandler
    : IRequestHandler<AddToCartCommand, BaseResponse<bool>>
    {
        private readonly ICartRepository _cartRepo;

        public AddToCartHandler(ICartRepository cartRepo)
        {
            _cartRepo = cartRepo;
        }

        public async Task<BaseResponse<bool>> Handle(AddToCartCommand request, CancellationToken cancellationToken)
        {
            var cart = await _cartRepo.GetCartWithItemsAsync(request.UserOrSessionId);

            if (cart == null)
            {
                cart = new Cart
                {
                    SessionId = request.UserOrSessionId,
                    CreatedAt = DateTime.Now
                };
                await _cartRepo.AddAsync(cart);
                await _cartRepo.SaveChangeAsync();
            }

            var existed = cart.Items.FirstOrDefault(i => i.ProductColorId == request.ProductColorId);

            if (existed != null)
            {
                existed.Quantity += request.Quantity;
            }
            else
            {
                var newItem = new CartItem
                {
                    ProductColorId = request.ProductColorId,
                    Quantity = request.Quantity,
                    UnitPrice = 50, // TODO: get price from product
                    CartId = cart.Id
                };

                await _cartRepo.AddCartItemAsync(newItem, cancellationToken);
            }

            await _cartRepo.SaveChangeAsync();

            return BaseResponse<bool>.SuccessResponse(true, "Added to cart");
        }
    }

}
