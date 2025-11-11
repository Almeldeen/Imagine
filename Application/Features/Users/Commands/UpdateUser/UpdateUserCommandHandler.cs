using Application.Common.Models;
using MediatR;

namespace Application.Features.Users.Commands.UpdateUser
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, BaseResponse<bool>>
    {
        public async Task<BaseResponse<bool>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
