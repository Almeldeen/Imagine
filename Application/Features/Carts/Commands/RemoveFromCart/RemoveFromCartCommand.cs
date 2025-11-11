using Application.Common.Models;
using MediatR;

namespace Application.Features.Carts.Commands.RemoveFromCart
{
    public class RemoveFromCartCommand : IRequest<BaseResponse<bool>>
    {
    }
}
