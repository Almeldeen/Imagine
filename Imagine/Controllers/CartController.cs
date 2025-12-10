using Application.Common.Models;
using Application.Features.Carts.Commands.AddToCart;
using Application.Features.Carts.Commands.AddCustomProductToCart;
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

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart(AddToCartCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("add-custom")]
        public async Task<IActionResult> AddCustomProductToCart(AddCustomProductToCartCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }


        // 🔴 Remove Item
        [HttpDelete("remove/{itemId}")]
        public async Task<IActionResult> RemoveItem(int itemId)
        {
            var result = await _mediator.Send(new RemoveCartItemCommand(itemId));
            return Ok(result);
        }

        // 🧹 Clear Cart
        [HttpDelete("clear/{userOrSessionId}")]
        public async Task<IActionResult> ClearCart(string userOrSessionId)
        {
            var result = await _mediator.Send(new ClearCartCommand(userOrSessionId));
            return Ok(result);
        }
        [HttpPut("update-quantity")]
        public async Task<IActionResult> UpdateQuantity(UpdateCartItemQuantityCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
