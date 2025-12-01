using Application.Common.Models;
using MediatR;

namespace Application.Features.Carts.Commands.RemoveFromCart
{
    public record RemoveCartItemCommand(int ItemId)
     : IRequest<BaseResponse<bool>>;
}
