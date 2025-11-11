using Application.Common.Models;
using MediatR;

namespace Application.Features.Users.Commands.RegisterUser
{
    public class RegisterUserCommand : IRequest<BaseResponse<int>>
    {
    }
}
