using Application.Common.Models;
using Application.Features.AdminDashboard.DTOs;
using Application.Features.AdminDashboard.Queries.GetDashboardStats;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Imagine.Controllers
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("stats")]
        [ProducesResponseType(typeof(BaseResponse<AdminDashboardStatsDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<BaseResponse<AdminDashboardStatsDto>>> GetStats(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetDashboardStatsQuery(), cancellationToken);
            return Ok(result);
        }
    }
}
