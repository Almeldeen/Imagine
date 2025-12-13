using Application.Common.Models;
using MediatR;

namespace Application.Features.Carts.Commands.AddCustomProductToCart
{
    public record AddCustomProductToCartCommand(
        string UserOrSessionId,
        int CustomProductId,
        int Quantity
    ) : IRequest<BaseResponse<bool>>
    {
        public string? Size { get; init; }
    }
}
