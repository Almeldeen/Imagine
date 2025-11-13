using Application.Common.Models;
using Application.Features.Carts.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Carts.Queries.GetUserCart
{
    public class GetAllCartsQuery: IRequest<BaseResponse<List<CartItemDto>>>
    {
    }
}
