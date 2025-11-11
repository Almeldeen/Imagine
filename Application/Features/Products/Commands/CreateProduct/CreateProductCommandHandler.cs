using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Commands.CreateProduct
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, BaseResponse<int>>
    {
        public async Task<BaseResponse<int>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
