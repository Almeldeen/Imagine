using Application.Common.Models;
using MediatR;

namespace Application.Features.Orders.Commands.CreateOrder
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, BaseResponse<int>>
    {
        public async Task<BaseResponse<int>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
