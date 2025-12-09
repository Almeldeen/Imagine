using Application.Common.Models;
using Application.Features.Orders.DTOs;
using Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Orders.Queries.GetOrderById
{
    public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, BaseResponse<AdminOrderDto>>
    {
        private readonly IOrderRepository _orderRepository;

        public GetOrderByIdQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<BaseResponse<AdminOrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            if (request.Id <= 0)
            {
                return BaseResponse<AdminOrderDto>.FailureResponse("A valid order id is required.");
            }

            var orderQuery = _orderRepository
                .GetAllQueryable()
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .AsNoTracking()
                .Where(o => o.Id == request.Id);

            var order = await orderQuery
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
                .FirstOrDefaultAsync(cancellationToken);

            if (order == null)
            {
                return BaseResponse<AdminOrderDto>.FailureResponse("Order not found.");
            }

            return BaseResponse<AdminOrderDto>.SuccessResponse(order, "Order retrieved successfully");
        }
    }
}
