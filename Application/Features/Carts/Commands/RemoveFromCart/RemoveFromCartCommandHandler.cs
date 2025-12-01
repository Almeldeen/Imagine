using Application.Common.Models;
using Core.Interfaces;
using MediatR;

namespace Application.Features.Carts.Commands.RemoveFromCart
{
    public class RemoveCartItemHandler
    : IRequestHandler<RemoveCartItemCommand, BaseResponse<bool>>
    {
        private readonly ICartItemRepository _cartItemRepo;

        public RemoveCartItemHandler(ICartItemRepository cartItemRepo)
        {
            _cartItemRepo = cartItemRepo;
        }

        public async Task<BaseResponse<bool>> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
        {
            var item = await _cartItemRepo.GetCartItemByIdAsync(request.ItemId);

            if (item == null)
                return BaseResponse<bool>.FailureResponse("Item not found");

            await _cartItemRepo.DeleteAsync(item);
          

            return BaseResponse<bool>.SuccessResponse(true, "Item removed");
        }
    }


}

