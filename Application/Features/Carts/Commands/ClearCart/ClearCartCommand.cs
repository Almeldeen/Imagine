using Application.Common.Models;
using MediatR;

namespace Application.Features.Carts.Commands.ClearCart
{
    public record ClearCartCommand(string UserOrSessionId)
     : IRequest<BaseResponse<bool>>;
}
