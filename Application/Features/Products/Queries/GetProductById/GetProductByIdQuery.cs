using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Queries.GetProductById
{
    public class GetProductByIdQuery : IRequest<BaseResponse<object>>
    {
    }
}
