using Application.Common.Models;
using Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Carts.Commands.ClearCart
{


    public class ClearCartHandler
    : IRequestHandler<ClearCartCommand, BaseResponse<bool>>
    {
        private readonly ICartRepository _cartRepo;

        public ClearCartHandler(ICartRepository cartRepo)
        {
            _cartRepo = cartRepo;
        }

        public async Task<BaseResponse<bool>> Handle(ClearCartCommand request, CancellationToken cancellationToken)
        {
            var cart = await _cartRepo.GetCartWithItemsAsync(request.UserOrSessionId);

            if (cart == null)
                return BaseResponse<bool>.FailureResponse("Cart not found");

            await _cartRepo.ClearCartAsync(cart.Id);

            return BaseResponse<bool>.SuccessResponse(true, "Cart cleared");
        }
    }

}

