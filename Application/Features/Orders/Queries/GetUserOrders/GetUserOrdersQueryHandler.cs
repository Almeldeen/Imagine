using Application.Common.Models;
using Application.Features.Orders.DTOs;
using Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Orders.Queries.GetUserOrders
{
    public class GetUserOrdersQueryHandler : IRequestHandler<GetUserOrdersQuery, BaseResponse<List<OrderDto>>>
    {
        private readonly IOrderRepository _orderRepository;

        public GetUserOrdersQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<BaseResponse<List<OrderDto>>> Handle(GetUserOrdersQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                return BaseResponse<List<OrderDto>>.FailureResponse("User id is required.");
            }

            var query = _orderRepository
                .GetAllQueryable()
                .Where(o => o.UserId == request.UserId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    OrderDate = o.OrderDate,
                    Status = o.Status.ToString(),
                    TotalAmount = o.TotalAmount,
                    ItemsCount = o.OrderItems.Count,
                    ThumbnailUrl = o.OrderItems
                        .OrderBy(oi => oi.Id)
                        .Select(oi => oi.ProductImageUrl)
                        .FirstOrDefault()
                });

            var orders = await query.ToListAsync(cancellationToken);

            return BaseResponse<List<OrderDto>>.SuccessResponse(orders, "User orders retrieved successfully");
        }
    }
}
