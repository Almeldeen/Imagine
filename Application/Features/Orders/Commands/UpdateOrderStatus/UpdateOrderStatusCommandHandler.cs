using Application.Common.Models;
using Application.Features.Orders.DTOs;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Orders.Commands.UpdateOrderStatus
{
    public class UpdateOrderStatusCommandHandler
        : IRequestHandler<UpdateOrderStatusCommand, BaseResponse<AdminOrderDto>>
    {
        private readonly IOrderRepository _orderRepository;

        public UpdateOrderStatusCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<BaseResponse<AdminOrderDto>> Handle(
            UpdateOrderStatusCommand request,
            CancellationToken cancellationToken)
        {
            if (request.OrderId <= 0)
            {
                return BaseResponse<AdminOrderDto>.FailureResponse("OrderId must be greater than 0.");
            }

            var order = await _orderRepository.GetWithDetailsByIdAsync(request.OrderId, cancellationToken);
            if (order == null)
            {
                return BaseResponse<AdminOrderDto>.FailureResponse("Order not found.");
            }

            if (!Enum.TryParse<OrderStatus>(request.Status, true, out var newStatus))
            {
                return BaseResponse<AdminOrderDto>.FailureResponse("Invalid status value.");
            }

            var currentStatus = order.Status;
            if (!IsValidTransition(currentStatus, newStatus))
            {
                return BaseResponse<AdminOrderDto>.FailureResponse(
                    $"Cannot change status from {currentStatus} to {newStatus}.");
            }

            order.Status = newStatus;

            if (!string.IsNullOrWhiteSpace(request.TrackingNumber))
            {
                order.TrackingNumber = request.TrackingNumber.Trim();
            }

            if (newStatus == OrderStatus.Shipped && order.ShippedAt == null)
                order.ShippedAt = DateTime.UtcNow;

            if (newStatus == OrderStatus.Delivered && order.DeliveredAt == null)
                order.DeliveredAt = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(order, cancellationToken);

            var dto = new AdminOrderDto
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                SubTotal = order.SubTotal,
                ShippingCost = order.ShippingCost,
                Tax = order.Tax,
                Status = order.Status.ToString(),
                PaymentStatus = order.PaidAt != null ? "Paid" : "Pending",
                PaymentMethod = order.PaymentMethod,
                UserId = order.UserId,
                UserName = order.User != null
                    ? (order.User.FirstName + " " + order.User.LastName).Trim()
                    : null,
                UserEmail = order.User?.Email,
                UserPhoneNumber = order.User?.PhoneNumber,
                ShippingAddress = order.ShippingAddress,
                ShippingCity = order.ShippingCity,
                ShippingPostalCode = order.ShippingPostalCode,
                ShippingCountry = order.ShippingCountry,
                ShippingPhone = order.ShippingPhone,
                TrackingNumber = order.TrackingNumber,
                PaidAt = order.PaidAt,
                ShippedAt = order.ShippedAt,
                DeliveredAt = order.DeliveredAt,
                Items = order.OrderItems
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
            };

            return BaseResponse<AdminOrderDto>.SuccessResponse(dto, "Order status updated successfully.");
        }

        private static bool IsValidTransition(OrderStatus current, OrderStatus next)
        {
            if (current == next) return false;

            return current switch
            {
                OrderStatus.Pending    => next == OrderStatus.Processing || next == OrderStatus.Cancelled,
                OrderStatus.Processing => next == OrderStatus.Shipped || next == OrderStatus.Cancelled,
                OrderStatus.Shipped    => next == OrderStatus.Delivered || next == OrderStatus.Cancelled,
                OrderStatus.Delivered  => next == OrderStatus.Refunded,
                OrderStatus.Cancelled  => next == OrderStatus.Refunded,
                OrderStatus.Refunded   => false,
                _                      => false
            };
        }
    }
}
