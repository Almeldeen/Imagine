using Application.Common.Models;
using Application.Features.Orders.Commands.CreateOrder;
using Application.Features.Orders.Commands.UpdateOrderStatus;
using Application.Features.Orders.DTOs;
using Application.Features.Orders.Queries.GetUserOrders;
using Application.Features.Orders.Queries.GetAllOrders;
using Application.Features.Orders.Queries.GetOrderById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Imagine.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("create")]
        [Authorize(Roles = "Client")]
        [ProducesResponseType(typeof(BaseResponse<OrderCreatedResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<OrderCreatedResponseDto>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BaseResponse<OrderCreatedResponseDto>>> Create([FromBody] CreateOrderRequestDto dto, CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                         User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest(BaseResponse<OrderCreatedResponseDto>.FailureResponse("User id was not found in the access token."));
            }

            var command = new CreateOrderCommand
            {
                UserId = userId,
                Request = dto
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("mine")]
        [Authorize(Roles = "Client")]
        [ProducesResponseType(typeof(BaseResponse<List<OrderDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<List<OrderDto>>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BaseResponse<List<OrderDto>>>> GetMine(CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                         User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest(BaseResponse<List<OrderDto>>.FailureResponse("User id was not found in the access token."));
            }

            var query = new GetUserOrdersQuery
            {
                UserId = userId
            };

            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(BaseResponse<List<AdminOrderDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<List<AdminOrderDto>>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BaseResponse<List<AdminOrderDto>>>> GetAllForAdmin(
            [FromQuery] GetAllOrdersQuery query,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        [HttpPut("{id:int}/status")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(BaseResponse<AdminOrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<AdminOrderDto>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BaseResponse<AdminOrderDto>>> UpdateStatus(
            int id,
            [FromBody] UpdateOrderStatusRequestDto dto,
            CancellationToken cancellationToken)
        {
            var command = new UpdateOrderStatusCommand
            {
                OrderId = id,
                Status = dto.Status,
                TrackingNumber = dto.TrackingNumber
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(BaseResponse<AdminOrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<AdminOrderDto>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BaseResponse<AdminOrderDto>>> GetById(int id, CancellationToken cancellationToken)
        {
            var query = new GetOrderByIdQuery
            {
                Id = id
            };

            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }
    }
}

