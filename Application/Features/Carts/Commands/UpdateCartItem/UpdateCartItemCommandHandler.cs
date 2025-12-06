using Application.Common.Models;
using Application.Features.Carts.DTOs;
using Core.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Carts.Commands.UpdateCartItem
{
    public class UpdateCartItemQuantityHandler
      : IRequestHandler<UpdateCartItemQuantityCommand, BaseResponse<bool>>
    {
        private readonly ICartItemRepository _cartItemRepo;

        public UpdateCartItemQuantityHandler(ICartItemRepository cartItemRepo)
        {
            _cartItemRepo = cartItemRepo;
        }

        public async Task<BaseResponse<bool>> Handle(UpdateCartItemQuantityCommand request, CancellationToken cancellationToken)
        {
            var item = await _cartItemRepo.GetCartItemByIdAsync(request.ItemId);

            if (item == null)
                return BaseResponse<bool>.FailureResponse("Item not found");

            item.Quantity = request.Quantity;
            item.TotalPrice = item.UnitPrice * item.Quantity;

            await _cartItemRepo.UpdateCartItemAsync(item, cancellationToken);

            return BaseResponse<bool>.SuccessResponse(true, "Quantity updated");
        }
    }

}
