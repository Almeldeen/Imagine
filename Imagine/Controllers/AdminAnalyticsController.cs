using Application.Common.Models;
using Application.Features.AdminAnalytics.DTOs;
using Application.Features.AdminAnalytics.Queries.GetSalesOverview;
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
    public class AnalyticsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AnalyticsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("sales-overview")]
        [ProducesResponseType(typeof(BaseResponse<SalesOverviewDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<BaseResponse<SalesOverviewDto>>> GetSalesOverview([FromQuery] string period = "month", CancellationToken cancellationToken = default)
        {
            var query = new GetSalesOverviewQuery { Period = period };
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
    }
}
