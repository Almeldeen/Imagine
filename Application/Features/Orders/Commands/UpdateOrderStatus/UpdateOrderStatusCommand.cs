using Application.Common.Models;
using Application.Features.Orders.DTOs;
using MediatR;

namespace Application.Features.Orders.Commands.UpdateOrderStatus
{
    public class UpdateOrderStatusCommand : IRequest<BaseResponse<AdminOrderDto>>
    {
        public int OrderId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? TrackingNumber { get; set; }
    }
}
