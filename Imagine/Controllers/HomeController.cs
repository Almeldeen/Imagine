using Application.Common.Models;
using Application.Features.Home.DTOs;
using Application.Features.Home.Queries.GetHomeData;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Imagine.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class HomeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public HomeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<HomeDataDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<BaseResponse<HomeDataDto>>> Get(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetHomeDataQuery(), cancellationToken);
            return Ok(result);
        }
    }
}
