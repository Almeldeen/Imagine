using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Commands.UpdateProduct
{
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, BaseResponse<bool>>
    {
        public async Task<BaseResponse<bool>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
