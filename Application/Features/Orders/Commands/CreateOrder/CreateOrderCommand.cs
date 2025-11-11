using Application.Common.Models;
using MediatR;

namespace Application.Features.Orders.Commands.CreateOrder
{
    public class CreateOrderCommand : IRequest<BaseResponse<int>>
    {
    }
}
