using Application.Common.Models;
using MediatR;

namespace Application.Features.Users.Queries.GetUserById
{
    public class GetUserByIdQuery : IRequest<BaseResponse<object>>
    {
    }
}
