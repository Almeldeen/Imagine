using Application.Common.Enums;
using Application.Common.Models;
using Application.Features.Categories.Commands.CreateCategory;
using Application.Features.Categories.DTOs;
using Application.Features.Categories.Queries.GetCategoriesList;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Imagine.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CategoriesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CategoriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult<BaseResponse<int>>> CreateCategory([FromBody] CreateCategoryCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("GetCategories")]
        public async Task<ActionResult<BaseResponse<List<CategoryDto>>>> GetCategories(
             GetCategoriesListQuery query,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }
    }
}
