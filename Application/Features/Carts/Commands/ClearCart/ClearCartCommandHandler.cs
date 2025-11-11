using Application.Common.Models;
using MediatR;

namespace Application.Features.Carts.Commands.ClearCart
{
    public class ClearCartCommandHandler : IRequestHandler<ClearCartCommand, BaseResponse<bool>>
    {
        public async Task<BaseResponse<bool>> Handle(ClearCartCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
