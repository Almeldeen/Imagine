using Application.Common.Models;
using Application.Features.Orders.DTOs;
using MediatR;

namespace Application.Features.Orders.Commands.CreateOrder
{
    public class CreateOrderCommand : IRequest<BaseResponse<OrderCreatedResponseDto>>
    {
        public string UserId { get; set; } = string.Empty;
        public CreateOrderRequestDto Request { get; set; } = new CreateOrderRequestDto();
    }
}
