using Application.Common.Models;
using Application.Features.Orders.DTOs;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.Orders.Queries.GetUserOrders
{
    public class GetUserOrdersQuery : IRequest<BaseResponse<List<OrderDto>>>
    {
        public string UserId { get; set; } = string.Empty;
    }
}
