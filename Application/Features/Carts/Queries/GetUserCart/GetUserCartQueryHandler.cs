using Application.Common.Models;
using MediatR;

namespace Application.Features.Carts.Queries.GetUserCart
{
    public class GetUserCartQueryHandler : IRequestHandler<GetUserCartQuery, BaseResponse<object>>
    {
        public async Task<BaseResponse<object>> Handle(GetUserCartQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
