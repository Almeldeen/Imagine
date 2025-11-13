using Application.Common.Models;
using Application.Features.Carts.DTOs;
using MediatR;

namespace Application.Features.Carts.Commands.AddToCart
{
    public class AddToCartCommand : IRequest<BaseResponse<CartDto>>
    {
        public AddCartDto Cart { get; set; } = new();

    }
}
