using Application.Common.Models;
using Application.Features.Carts.Commands.AddToCart;
using Application.Features.Carts.Commands.ClearCart;
using Application.Features.Carts.Commands.RemoveFromCart;
using Application.Features.Carts.Commands.UpdateCartItem;
using Application.Features.Carts.DTOs;
using Application.Features.Carts.Queries.GetUserCart;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Imagine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CartController(IMediator mediator)
        {
            _mediator = mediator;
        }

      
        [HttpGet("all")]
        public async Task<IActionResult> GetAllCartItems()
        {
            var response = await _mediator.Send(new GetAllCartsQuery());
            return Ok(response);
        }

        [HttpGet("userOrSessionId")]
        public async Task<IActionResult> GetCartWithItemWithId(string userOrSessionId)
        {
            var response = await _mediator.Send(new GetUserCartQuery { UserOrSessionId = userOrSessionId });
            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<BaseResponse<int>>> CreateCart([FromBody] AddCartDto addCartDto)
        {
            var command = new AddToCartCommand { Cart = addCartDto };
            var result = await _mediator.Send(command);
            return Ok(result);
        }


        [HttpDelete("clear")]
        public async Task<ActionResult<BaseResponse<bool>>> ClearCart([FromQuery] string sessionId)
        {
            var result = await _mediator.Send(new ClearCartCommand { SessionId = sessionId });
            return Ok(result);
        }

        [HttpDelete("remove")]
        public async Task<ActionResult<BaseResponse<bool>>> RemoveFromCart([FromQuery] int cartItemId)
        {
            var result = await _mediator.Send(new RemoveFromCartCommand { CartItemId = cartItemId });
            return Ok(result);
        }
        [HttpPut("item")]
        public async Task<ActionResult<BaseResponse<CartDto>>> UpdateCartItem([FromBody] UpdateCartItemCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
