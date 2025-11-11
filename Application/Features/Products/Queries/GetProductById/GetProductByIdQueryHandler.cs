using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Queries.GetProductById
{
    public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, BaseResponse<object>>
    {
        public async Task<BaseResponse<object>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
