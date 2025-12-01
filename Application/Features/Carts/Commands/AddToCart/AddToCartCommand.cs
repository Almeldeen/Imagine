using Application.Common.Models;
using Application.Features.Carts.DTOs;
using MediatR;

namespace Application.Features.Carts.Commands.AddToCart
{
    public record AddToCartCommand(
      string UserOrSessionId,
      int ProductColorId,
      int Quantity
  ) : IRequest<BaseResponse<bool>>;
}
