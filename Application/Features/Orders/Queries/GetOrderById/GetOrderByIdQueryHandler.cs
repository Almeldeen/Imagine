using Application.Common.Models;
using MediatR;

namespace Application.Features.Orders.Queries.GetOrderById
{
    public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, BaseResponse<object>>
    {
        public async Task<BaseResponse<object>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
