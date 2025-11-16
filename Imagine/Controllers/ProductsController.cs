using Application.Common.Models;
using Application.Features.Products.Commands.CreateProduct;
using Application.Features.Products.Commands.DeleteProduct;
using Application.Features.Products.Commands.UpdateProduct;
using Application.Features.Products.DTOs;
using Application.Features.Products.Queries.GetProductById;
using Application.Features.Products.Queries.GetProductsList;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Imagine.Controllers
{
    // Controller خاص بمنتجات، يوفر CRUD + الاستعلام مع Pagination/Search/Sorting
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ProductsController(IMediator mediator) => _mediator = mediator;

        // نماذج استقبال بيانات الفورم في واجهات POST/PUT (تُحوّل لاحقًا إلى Commands)
        public class CreateProductForm
        {
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
            public decimal Price { get; set; }
            public bool IsActive { get; set; } = true;
            public int CategoryId { get; set; }
            public IFormFile? ImageFile { get; set; }
        }

        public class UpdateProductForm
        {
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
            public decimal Price { get; set; }
            public bool IsActive { get; set; } = true;
            public int CategoryId { get; set; }
            public IFormFile? ImageFile { get; set; }
        }

        // إنشاء منتج جديد مع رفع صورة اختيارية
        [HttpPost]
        [ProducesResponseType(typeof(BaseResponse<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<int>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BaseResponse<int>>> Create([FromForm] CreateProductForm form, CancellationToken ct)
        {
            var cmd = new CreateProductCommand
            {
                Name = form.Name,
                Description = form.Description,
                Price = form.Price,
                IsActive = form.IsActive,
                CategoryId = form.CategoryId,
                ImageStream = form.ImageFile?.OpenReadStream(),
                ImageFileName = form.ImageFile?.FileName
            };

            var result = await _mediator.Send(cmd, ct);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        // تحديث منتج موجود مع إمكانية استبدال الصورة
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BaseResponse<bool>>> Update([FromRoute] int id, [FromForm] UpdateProductForm form, CancellationToken ct)
        {
            var cmd = new UpdateProductCommand
            {
                Id = id,
                Name = form.Name,
                Description = form.Description,
                Price = form.Price,
                IsActive = form.IsActive,
                CategoryId = form.CategoryId,
                NewImageStream = form.ImageFile?.OpenReadStream(),
                NewImageFileName = form.ImageFile?.FileName
            };

            var result = await _mediator.Send(cmd, ct);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        // حذف منتج وصورته (إن وجدت)
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BaseResponse<bool>>> Delete([FromRoute] int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new DeleteProductCommand { Id = id }, ct);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        // إرجاع منتج واحد حسب المعرّف
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(BaseResponse<ProductDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<BaseResponse<ProductDto>>> GetById([FromRoute] int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetProductByIdQuery { Id = id }, ct);
            return Ok(result);
        }

        // إرجاع قائمة منتجات مع دعم البحث والفرز والتقسيم إلى صفحات
        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<List<ProductDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<BaseResponse<List<ProductDto>>>> GetList([FromQuery] GetProductsListQuery query, CancellationToken ct)
        {
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
    }
}
