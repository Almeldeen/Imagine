using Application.Common.Models;
using MediatR;

namespace Application.Features.Users.Commands.ResetCustomerPassword
{
    public class ResetCustomerPasswordCommand : IRequest<BaseResponse<string>>
    {
        public string UserId { get; set; } = string.Empty;
    }
}
