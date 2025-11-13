using Application.Common.Models;
using MediatR;

namespace Application.Features.Carts.Commands.ClearCart
{
    public class ClearCartCommand : IRequest<BaseResponse<bool>>
    {
        public string SessionId { get; set; } = string.Empty;
    }
}
