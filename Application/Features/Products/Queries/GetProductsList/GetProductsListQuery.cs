using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Queries.GetProductsList
{
    public class GetProductsListQuery : IRequest<BaseResponse<object>>
    {
    }
}
