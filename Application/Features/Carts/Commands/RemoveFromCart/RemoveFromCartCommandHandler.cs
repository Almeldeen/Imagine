using Application.Common.Models;
using MediatR;

namespace Application.Features.Carts.Commands.RemoveFromCart
{
    public class RemoveFromCartCommandHandler : IRequestHandler<RemoveFromCartCommand, BaseResponse<bool>>
    {
        public async Task<BaseResponse<bool>> Handle(RemoveFromCartCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
