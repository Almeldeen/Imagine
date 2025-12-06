using Application.Common.Models;
using Application.Features.Users.DTOs;
using MediatR;

namespace Application.Features.Users.Queries.LoginUser
{
    public class LoginUserQuery : IRequest<BaseResponse<LoginResultDto>>
    {
        public LoginRequestDto Request { get; set; } = new LoginRequestDto();
    }
}
