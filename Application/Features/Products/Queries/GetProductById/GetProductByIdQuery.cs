using Application.Common.Models;
using Application.Features.Products.DTOs;
using MediatR;

namespace Application.Features.Products.Queries.GetProductById
{
    public class GetProductByIdQuery : IRequest<BaseResponse<ProductDetailsDto>>
    {
        // Target product Id
        public int Id { get; set; }
    }
}
