using Application.Common.Models;
using MediatR;

namespace Application.Features.Carts.Queries.GetUserCart
{
    public class GetUserCartQuery : IRequest<BaseResponse<object>>
    {
    }
}
