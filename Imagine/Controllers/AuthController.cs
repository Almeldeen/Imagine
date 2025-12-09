using Application.Common.Models;
using Application.Features.Users.Commands.RegisterUser;
using Application.Features.Users.Commands.UpdateProfileImage;
using Application.Features.Users.DTOs;
using Application.Features.Users.Queries.LoginUser;
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
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public class RegisterForm
        {
            public string? FullName { get; set; }
            public string Email { get; set; } = string.Empty;
            public string PhoneNumber { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string ConfirmPassword { get; set; } = string.Empty;
            public IFormFile? ProfileImageFile { get; set; }
        }

        [HttpPost("register")]
        public async Task<ActionResult<BaseResponse<string>>> Register([FromForm] RegisterForm form, CancellationToken cancellationToken)
        {
            var dto = new RegisterRequestDto
            {
                FullName = form.FullName,
                Email = form.Email,
                PhoneNumber = form.PhoneNumber,
                Password = form.Password,
                ConfirmPassword = form.ConfirmPassword
            };

            var command = new RegisterUserCommand
            {
                Request = dto,
                ProfileImageStream = form.ProfileImageFile?.OpenReadStream(),
                ProfileImageFileName = form.ProfileImageFile?.FileName
            };

            var result = await _mediator.Send(command, cancellationToken);

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

        [HttpPost("profile-image")]
        [Authorize]
        public async Task<ActionResult<BaseResponse<string>>> UpdateProfileImage([FromForm] IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(BaseResponse<string>.FailureResponse("Image file is required."));
            }

            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                         User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest(BaseResponse<string>.FailureResponse("User id was not found in the access token."));
            }

            var command = new UpdateProfileImageCommand
            {
                UserId = userId,
                ImageStream = file.OpenReadStream(),
                FileName = file.FileName
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
