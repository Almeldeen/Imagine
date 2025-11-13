using Application.Common.Models;
using Application.Features.Carts.DTOs;
using MediatR;

namespace Application.Features.Carts.Queries.GetUserCart
{
   
    public class GetUserCartQuery : IRequest<BaseResponse<CartDto>>
    {
        public string UserOrSessionId { get; set; } = string.Empty;
    }
}
