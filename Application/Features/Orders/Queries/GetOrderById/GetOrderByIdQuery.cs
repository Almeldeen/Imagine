using Application.Common.Models;
using MediatR;

namespace Application.Features.Orders.Queries.GetOrderById
{
    public class GetOrderByIdQuery : IRequest<BaseResponse<object>>
    {
    }
}
