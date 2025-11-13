using Application.Common.Models;
using Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Carts.Commands.ClearCart
{
    public class ClearCartCommandHandler : IRequestHandler<ClearCartCommand, BaseResponse<bool>>
    {
        private readonly ICartRepository _cartRepo;
        private readonly IHttpContextAccessor _httpContext;

        public ClearCartCommandHandler(ICartRepository cartRepo, IHttpContextAccessor httpContext)
        {
            _cartRepo = cartRepo;
            _httpContext = httpContext;
        }

        public async Task<BaseResponse<bool>> Handle(ClearCartCommand request, CancellationToken cancellationToken)
        {
            
            var cart = await _cartRepo.GetCartWithItemsAsync(request.SessionId);

            if (cart == null)
                return BaseResponse<bool>.FailureResponse("Cart not found");

            await _cartRepo.ClearCartAsync(cart.Id);
            return BaseResponse<bool>.SuccessResponse(true, "Cart cleared successfully");
        }

    }
}

