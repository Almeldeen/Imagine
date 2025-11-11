using Application.Common.Models;
using MediatR;

namespace Application.Features.Users.Queries.GetUserById
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, BaseResponse<object>>
    {
        public async Task<BaseResponse<object>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
