using Application.Common.Models;
using Application.Features.Users.Commands.UpdateCustomer;
using Application.Features.Users.Commands.ResetCustomerPassword;
using Application.Features.Users.Commands.ImportCustomers;
using Application.Features.Users.DTOs;
using Application.Features.Users.Queries.GetCustomerById;
using Application.Features.Users.Queries.GetCustomers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Imagine.Controllers
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin")]
    public class CustomersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CustomersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public class UpdateCustomerForm
        {
            public string FullName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string? PhoneNumber { get; set; }
            public bool IsActive { get; set; } = true;
            public List<string> Roles { get; set; } = new();
            public IFormFile? ProfileImageFile { get; set; }
        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<CustomerListResultDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<BaseResponse<CustomerListResultDto>>> GetAll([FromQuery] GetCustomersQuery query, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResponse<CustomerDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<CustomerDetailsDto>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BaseResponse<CustomerDetailsDto>>> GetById(string id, CancellationToken cancellationToken)
        {
            var query = new GetCustomerByIdQuery
            {
                Id = id
            };

            var result = await _mediator.Send(query, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BaseResponse<bool>>> Update(
            string id,
            [FromForm] UpdateCustomerForm form,
            CancellationToken cancellationToken)
        {
            var dto = new UpdateCustomerDto
            {
                FullName = form.FullName,
                Email = form.Email,
                PhoneNumber = form.PhoneNumber,
                IsActive = form.IsActive,
                Roles = form.Roles,
            };

            Stream? imageStream = null;
            string? fileName = null;

            if (form.ProfileImageFile != null && form.ProfileImageFile.Length > 0)
            {
                imageStream = form.ProfileImageFile.OpenReadStream();
                fileName = form.ProfileImageFile.FileName;
            }

            var command = new UpdateCustomerCommand
            {
                Id = id,
                Request = dto,
                ProfileImageStream = imageStream,
                ProfileImageFileName = fileName,
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("{id}/reset-password")]
        [ProducesResponseType(typeof(BaseResponse<PasswordResetResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<PasswordResetResultDto>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BaseResponse<PasswordResetResultDto>>> ResetPassword(string id, CancellationToken cancellationToken)
        {
            var command = new ResetCustomerPasswordCommand
            {
                UserId = id
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
