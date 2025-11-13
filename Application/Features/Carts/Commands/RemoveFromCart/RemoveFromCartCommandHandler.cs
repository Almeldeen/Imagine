using Application.Common.Models;
using Core.Interfaces;
using MediatR;

namespace Application.Features.Carts.Commands.RemoveFromCart
{
    public class RemoveFromCartCommandHandler : IRequestHandler<RemoveFromCartCommand, BaseResponse<bool>>
    {
        private readonly ICartItemRepository _cartItemRepo;

        public RemoveFromCartCommandHandler(ICartItemRepository cartItemRepo)
        {
            _cartItemRepo = cartItemRepo;
        }

        public async Task<BaseResponse<bool>> Handle(RemoveFromCartCommand request, CancellationToken cancellationToken)
        {
            var item = await _cartItemRepo.GetByIdAsync(request.CartItemId, cancellationToken);
            if (item == null)
                return BaseResponse<bool>.FailureResponse("Cart item not found");

            await _cartItemRepo.DeleteAsync(item, cancellationToken);
            return BaseResponse<bool>.SuccessResponse(true, "Cart item removed successfully");
        }
    }
}

