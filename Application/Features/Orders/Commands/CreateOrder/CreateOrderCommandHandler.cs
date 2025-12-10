using Application.Common.Models;
using Application.Features.Orders.DTOs;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Orders.Commands.CreateOrder
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, BaseResponse<OrderCreatedResponseDto>>
    {
        private readonly ICartRepository _cartRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;

        public CreateOrderCommandHandler(
            ICartRepository cartRepository,
            IOrderRepository orderRepository,
            IOrderItemRepository orderItemRepository)
        {
            _cartRepository = cartRepository;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
        }

        public async Task<BaseResponse<OrderCreatedResponseDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            if (request == null || request.Request == null)
            {
                return BaseResponse<OrderCreatedResponseDto>.FailureResponse("Order details are required.");
            }

            var dto = request.Request;

            var cart = await _cartRepository.GetCartWithItemsAsync(dto.CartUserOrSessionId);
            if (cart == null || cart.Items == null || cart.Items.Count == 0)
            {
                return BaseResponse<OrderCreatedResponseDto>.FailureResponse("Cart is empty.");
            }

            var cartKey = dto.CartUserOrSessionId ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(cart.UserId) &&
                string.Equals(cart.UserId, cartKey, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(cart.UserId, request.UserId, StringComparison.OrdinalIgnoreCase))
            {
                return BaseResponse<OrderCreatedResponseDto>.FailureResponse("Cart does not belong to the current user.");
            }

            cart.UserId = request.UserId;

            var subTotal = cart.Items.Sum(i => i.UnitPrice * i.Quantity);
            var shipping = cart.Items.Any() ? 9.99m : 0m;
            var tax = Math.Round(subTotal * 0.15m, 2, MidpointRounding.AwayFromZero);
            var total = subTotal + shipping + tax;

            var order = new Order
            {
                UserId = request.UserId,
                OrderNumber = GenerateOrderNumber(),
                SubTotal = subTotal,
                ShippingCost = shipping,
                Tax = tax,
                TotalAmount = total,
                Status = OrderStatus.Pending,
                ShippingAddress = dto.Address,
                ShippingCity = dto.City,
                ShippingPostalCode = "N/A",
                ShippingCountry = "N/A",
                ShippingPhone = dto.PhoneNumber
            };

            foreach (var item in cart.Items)
            {
                var productName = item.ProductColor?.Product?.Name
                                  ?? item.CustomProduct?.Product?.Name
                                  ?? "Item";

                var colorName = item.ProductColor?.ColorName
                                ?? item.CustomProduct?.CustomColors?.FirstOrDefault()?.ColorName;

                var imageUrl = item.ProductColor?.Images?.FirstOrDefault(i => i.IsMain)?.ImageUrl
                               ?? item.CustomProduct?.AIRenderedPreviewUrl
                               ?? item.CustomProduct?.CustomDesignImageUrl;

                var unitPrice = item.UnitPrice;
                var totalPrice = unitPrice * item.Quantity;

                var orderItem = new OrderItem
                {
                    ProductColorId = item.ProductColorId,
                    CustomProductId = item.CustomProductId,
                    ProductName = productName,
                    ColorName = colorName,
                    ProductImageUrl = imageUrl,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice,
                };

                order.OrderItems.Add(orderItem);
            }

            var created = await _orderRepository.AddAsync(order, cancellationToken);

            await _cartRepository.ClearCartAsync(cart.Id);

            var responseDto = new OrderCreatedResponseDto
            {
                OrderId = created.Id,
                OrderNumber = created.OrderNumber,
                TotalAmount = created.TotalAmount
            };

            return BaseResponse<OrderCreatedResponseDto>.SuccessResponse(responseDto, "Order created successfully");
        }

        private static string GenerateOrderNumber()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpperInvariant();
            return $"INV-{timestamp}-{random}";
        }
    }
}
