using Application.Common.Models;
using MediatR;

namespace Application.Features.Orders.Commands.CancelOrder
{
    public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, BaseResponse<bool>>
    {
        public async Task<BaseResponse<bool>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
