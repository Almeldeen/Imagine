using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Commands.CreateProduct
{
    public class CreateProductCommand : IRequest<BaseResponse<int>>
    {
    }
}
