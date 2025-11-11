using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Commands.UpdateProduct
{
    public class UpdateProductCommand : IRequest<BaseResponse<bool>>
    {
    }
}
