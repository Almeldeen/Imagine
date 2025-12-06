using Application.Common.Models;
using Application.Features.Products.DTOs;
using MediatR;

namespace Application.Features.Products.Queries.GetFeaturedProducts
{
    public class GetFeaturedProductsQuery : IRequest<BaseResponse<List<ProductListDto>>>
    {
        public int Take { get; set; } = 8;
    }
}
