using Application.Common.Models;
using MediatR;

namespace Application.Features.Orders.Commands.UpdateOrderStatus
{
    public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, BaseResponse<bool>>
    {
        public async Task<BaseResponse<bool>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
