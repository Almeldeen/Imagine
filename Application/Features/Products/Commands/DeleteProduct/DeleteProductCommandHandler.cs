using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Commands.DeleteProduct
{
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, BaseResponse<bool>>
    {
        public async Task<BaseResponse<bool>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
