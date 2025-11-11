using Application.Common.Models;
using MediatR;

namespace Application.Features.Orders.Queries.GetUserOrders
{
    public class GetUserOrdersQuery : IRequest<BaseResponse<object>>
    {
    }
}
