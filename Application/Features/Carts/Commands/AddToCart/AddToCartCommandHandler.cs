using Application.Common.Models;
using MediatR;

namespace Application.Features.Carts.Commands.AddToCart
{
    public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, BaseResponse<bool>>
    {
        public async Task<BaseResponse<bool>> Handle(AddToCartCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
