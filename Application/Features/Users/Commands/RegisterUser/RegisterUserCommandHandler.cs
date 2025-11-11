using Application.Common.Models;
using MediatR;

namespace Application.Features.Users.Commands.RegisterUser
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, BaseResponse<int>>
    {
        public async Task<BaseResponse<int>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
