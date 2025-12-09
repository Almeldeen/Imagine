using Application.Common.Models;
using Application.Features.Orders.DTOs;
using MediatR;

namespace Application.Features.Orders.Queries.GetOrderById
{
    public class GetOrderByIdQuery : IRequest<BaseResponse<AdminOrderDto>>
    {
        public int Id { get; set; }
    }
}
