using Application.Common.Models;
using Application.Features.Users.Commands.RegisterUser;
using Application.Features.Users.DTOs;
using Application.Features.Users.Queries.LoginUser;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Imagine.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        public async Task<ActionResult<BaseResponse<string>>> Register([FromBody] RegisterRequestDto dto)
        {
            var command = new RegisterUserCommand { Request = dto };
            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<BaseResponse<LoginResultDto>>> Login([FromBody] LoginRequestDto dto)
        {
            var query = new LoginUserQuery { Request = dto };
            var result = await _mediator.Send(query);

            if (!result.Success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }
    }
}
