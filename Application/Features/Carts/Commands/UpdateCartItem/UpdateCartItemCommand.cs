using Application.Common.Models;
using Application.Features.Carts.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Carts.Commands.UpdateCartItem
{
    public record UpdateCartItemQuantityCommand(int ItemId, int Quantity)
    : IRequest<BaseResponse<bool>>;
}
