using Application.Common.Models;
using MediatR;

namespace Application.Features.Orders.Commands.CancelOrder
{
    public class CancelOrderCommand : IRequest<BaseResponse<bool>>
    {
    }
}
