using Application.Common.Models;
using MediatR;

namespace Application.Features.Users.Commands.UpdateUser
{
    public class UpdateUserCommand : IRequest<BaseResponse<bool>>
    {
    }
}
