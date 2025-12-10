using Application.Common.Models;
using Application.Features.CustomProducts.Commands.SaveCustomizationAsCustomProduct;
using Application.Features.CustomProducts.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Imagine.Controllers
{
    [ApiController]
    [Route("api/custom-products")]
    [Produces("application/json")]
    [Authorize(Roles = "Client")]
    public class CustomProductsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CustomProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("save-from-customization")]
        [ProducesResponseType(typeof(BaseResponse<SavedCustomProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<SavedCustomProductDto>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BaseResponse<SavedCustomProductDto>>> SaveFromCustomization(
            [FromBody] SaveCustomizationAsCustomProductCommand command,
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                         User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest(BaseResponse<SavedCustomProductDto>.FailureResponse("User id was not found in the access token."));
            }

            command.UserId = userId;

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
