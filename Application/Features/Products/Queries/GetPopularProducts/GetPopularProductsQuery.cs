using Application.Common.Models;
using Application.Features.Products.DTOs;
using MediatR;

namespace Application.Features.Products.Queries.GetPopularProducts
{
    public class GetPopularProductsQuery : IRequest<BaseResponse<List<ProductListDto>>>
    {
        public int Take { get; set; } = 8;
    }
}
