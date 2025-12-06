using Application.Common.Models;
using Application.Features.Products.DTOs;
using MediatR;

namespace Application.Features.Products.Queries.GetLatestProducts
{
    public class GetLatestProductsQuery : IRequest<BaseResponse<List<ProductListDto>>>
    {
        public int Take { get; set; } = 8;
    }
}
