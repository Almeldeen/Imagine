using Application.Common.Models;
using Application.Features.Categories.Commands.CreateCategory;
using Application.Features.Categories.Commands.DeleteCategory;
using Application.Features.Categories.Commands.UpdateCategory;
using Application.Features.Categories.DTOs;
using Application.Features.Categories.Queries.GetCategoryById;
using Application.Features.Categories.Queries.GetCategoriesList;
using Application.Features.Categories.Queries.GetFeaturedCategories;
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

        // GET /api/categories
        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<List<CategoryDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<BaseResponse<List<CategoryDto>>>> GetCategories([FromQuery] GetCategoriesListQuery query, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("featured")]
        [ProducesResponseType(typeof(BaseResponse<List<CategoryDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<BaseResponse<List<CategoryDto>>>> GetFeaturedCategories([FromQuery] GetFeaturedCategoriesQuery query, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        // GET /api/categories/{id}
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(BaseResponse<CategoryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<BaseResponse<CategoryDto>>> GetCategoryById([FromRoute] int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetCategoryByIdQuery { Id = id }, cancellationToken);
            return Ok(result);
        }

        public class CreateCategoryForm
        {
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
            public bool IsActive { get; set; } = true;
            public int DisplayOrder { get; set; } = 0;
            public IFormFile? ImageFile { get; set; }
        }

        public class UpdateCategoryForm
        {
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
            public bool IsActive { get; set; } = true;
            public int DisplayOrder { get; set; } = 0;
            public IFormFile? ImageFile { get; set; }
        }

        // POST /api/categories
        [HttpPost]
        [ProducesResponseType(typeof(BaseResponse<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<int>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BaseResponse<int>>> CreateCategory([FromForm] CreateCategoryForm form, CancellationToken cancellationToken)
        {
            var command = new CreateCategoryCommand
            {
                Name = form.Name,
                Description = form.Description,
                IsActive = form.IsActive,
                DisplayOrder = form.DisplayOrder,
                ImageStream = form.ImageFile?.OpenReadStream(),
                ImageFileName = form.ImageFile?.FileName
            };

            var result = await _mediator.Send(command, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // PUT /api/categories/{id}
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BaseResponse<bool>>> UpdateCategory([FromRoute] int id, [FromForm] UpdateCategoryForm form, CancellationToken cancellationToken)
        {
            var command = new UpdateCategoryCommand
            {
                Id = id,
                Name = form.Name,
                Description = form.Description,
                IsActive = form.IsActive,
                DisplayOrder = form.DisplayOrder,
                NewImageStream = form.ImageFile?.OpenReadStream(),
                NewImageFileName = form.ImageFile?.FileName
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // DELETE /api/categories/{id}
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BaseResponse<bool>>> DeleteCategory([FromRoute] int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteCategoryCommand { Id = id }, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
