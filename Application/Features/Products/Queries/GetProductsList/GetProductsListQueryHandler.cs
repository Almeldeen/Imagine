using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Queries.GetProductsList
{
    public class GetProductsListQueryHandler : IRequestHandler<GetProductsListQuery, BaseResponse<object>>
    {
        public async Task<BaseResponse<object>> Handle(GetProductsListQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
