using Application.Common.Models;
using Application.Features.Users.DTOs;
using MediatR;
using System.IO;

namespace Application.Features.Users.Commands.RegisterUser
{
    public class RegisterUserCommand : IRequest<BaseResponse<string>>
    {
        public RegisterRequestDto Request { get; set; } = new RegisterRequestDto();
        public Stream? ProfileImageStream { get; set; }
        public string? ProfileImageFileName { get; set; }
    }
}
