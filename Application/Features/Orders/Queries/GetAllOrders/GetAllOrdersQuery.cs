using Application.Common.Models;
using Application.Features.Orders.DTOs;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.Orders.Queries.GetAllOrders
{
    public class GetAllOrdersQuery : IRequest<BaseResponse<List<AdminOrderDto>>>
    {
        // Free text search over order number, customer name, or email
        public string? SearchTerm { get; set; }

        // Optional status filter (Pending, Processing, Shipped, Delivered, Cancelled, Refunded, or "all")
        public string? Status { get; set; }

        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // Sort key: date-desc | date-asc | amount-desc | amount-asc | status | customer
        public string? SortKey { get; set; }
    }
}
