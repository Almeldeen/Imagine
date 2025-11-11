using Application.Common.Models;
using MediatR;

namespace Application.Features.Categories.Queries.GetCategoryById
{
    public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, BaseResponse<object>>
    {
        public async Task<BaseResponse<object>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
