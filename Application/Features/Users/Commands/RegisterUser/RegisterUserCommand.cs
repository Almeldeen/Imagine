using Application.Common.Models;
using Application.Features.Users.DTOs;
using MediatR;

namespace Application.Features.Users.Commands.RegisterUser
{
    public class RegisterUserCommand : IRequest<BaseResponse<string>>
    {
        public RegisterRequestDto Request { get; set; } = new RegisterRequestDto();
    }
}
