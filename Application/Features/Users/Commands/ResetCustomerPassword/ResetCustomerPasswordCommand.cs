using Application.Common.Models;
using Application.Features.Users.DTOs;
using MediatR;

namespace Application.Features.Users.Commands.ResetCustomerPassword
{
    public class ResetCustomerPasswordCommand : IRequest<BaseResponse<PasswordResetResultDto>>
    {
        public string UserId { get; set; } = string.Empty;
    }
}
