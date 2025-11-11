using Application.Common.Models;
using MediatR;

namespace Application.Features.Orders.Commands.UpdateOrderStatus
{
    public class UpdateOrderStatusCommand : IRequest<BaseResponse<bool>>
    {
    }
}
