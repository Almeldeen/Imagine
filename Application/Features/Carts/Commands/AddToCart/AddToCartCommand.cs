using Application.Common.Models;
using MediatR;

namespace Application.Features.Carts.Commands.AddToCart
{
    public class AddToCartCommand : IRequest<BaseResponse<bool>>
    {
    }
}
