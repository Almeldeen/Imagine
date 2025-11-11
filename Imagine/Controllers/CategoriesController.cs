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
        [ProducesResponseType(typeof(BaseResponse<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<int>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BaseResponse<int>>> CreateCategory(
            [FromBody] CreateCategoryCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<List<CategoryDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<BaseResponse<List<CategoryDto>>>> GetCategories(GetCategoriesListQuery query,
            CancellationToken cancellationToken = default)
        {

            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }
    }
}
