using Application.Common.Models;
using Application.Features.Orders.DTOs;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Orders.Queries.GetAllOrders
{
    public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, BaseResponse<List<AdminOrderDto>>>
    {
        private readonly IOrderRepository _orderRepository;

        public GetAllOrdersQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<BaseResponse<List<AdminOrderDto>>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
        {
            var query = _orderRepository
                .GetAllQueryable()
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .AsNoTracking();

            // Search by order number, customer name, or email
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim().ToLower();

                query = query.Where(o =>
                    o.OrderNumber.ToLower().Contains(term) ||
                    (o.User != null && (
                        ((o.User.FirstName + " " + o.User.LastName).Trim().ToLower().Contains(term)) ||
                        o.User.Email.ToLower().Contains(term))));
            }

            // Status filter (ignore "all")
            if (!string.IsNullOrWhiteSpace(request.Status) &&
                !string.Equals(request.Status, "all", StringComparison.OrdinalIgnoreCase))
            {
                if (Enum.TryParse<OrderStatus>(request.Status, true, out var status))
                {
                    query = query.Where(o => o.Status == status);
                }
            }

            // Count before pagination
            var totalItems = await query.CountAsync(cancellationToken);

            // Sorting
            switch (request.SortKey)
            {
                case "date-asc":
                    query = query.OrderBy(o => o.OrderDate);
                    break;
                case "amount-desc":
                    query = query.OrderByDescending(o => o.TotalAmount);
                    break;
                case "amount-asc":
                    query = query.OrderBy(o => o.TotalAmount);
                    break;
                case "status":
                    query = query.OrderBy(o => o.Status).ThenByDescending(o => o.OrderDate);
                    break;
                default:
                    // date-desc and any unknown
                    query = query.OrderByDescending(o => o.OrderDate);
                    break;
            }

            // Pagination
            var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
            var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

            query = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            var items = await query
                .Select(o => new AdminOrderDto
                {
                    OrderId = o.Id,
                    OrderNumber = o.OrderNumber,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    SubTotal = o.SubTotal,
                    ShippingCost = o.ShippingCost,
                    Tax = o.Tax,
                    Status = o.Status.ToString(),
                    PaymentStatus = o.PaidAt != null ? "Paid" : "Pending",
                    PaymentMethod = o.PaymentMethod,
                    UserId = o.UserId,
                    UserName = o.User != null
                        ? (o.User.FirstName + " " + o.User.LastName).Trim()
                        : null,
                    UserEmail = o.User != null ? o.User.Email : null,
                    UserPhoneNumber = o.User != null ? o.User.PhoneNumber : null,
                    ShippingAddress = o.ShippingAddress,
                    ShippingCity = o.ShippingCity,
                    ShippingPostalCode = o.ShippingPostalCode,
                    ShippingCountry = o.ShippingCountry,
                    ShippingPhone = o.ShippingPhone,
                    TrackingNumber = o.TrackingNumber,
                    PaidAt = o.PaidAt,
                    ShippedAt = o.ShippedAt,
                    DeliveredAt = o.DeliveredAt,
                    Items = o.OrderItems
                        .OrderBy(oi => oi.Id)
                        .Select(oi => new AdminOrderItemDto
                        {
                            ProductName = oi.ProductName,
                            ColorName = oi.ColorName,
                            Quantity = oi.Quantity,
                            UnitPrice = oi.UnitPrice,
                            TotalPrice = oi.TotalPrice,
                            ProductImageUrl = oi.ProductImageUrl
                        })
                        .ToList()
                })
                .ToListAsync(cancellationToken);

            return BaseResponse<List<AdminOrderDto>>.SuccessResponse(items, pageNumber, pageSize, totalItems, "Orders retrieved successfully");
        }
    }
}
