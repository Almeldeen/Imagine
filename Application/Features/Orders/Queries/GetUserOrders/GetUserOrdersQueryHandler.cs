using Application.Common.Models;
using MediatR;

namespace Application.Features.Orders.Queries.GetUserOrders
{
    public class GetUserOrdersQueryHandler : IRequestHandler<GetUserOrdersQuery, BaseResponse<object>>
    {
        public async Task<BaseResponse<object>> Handle(GetUserOrdersQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
