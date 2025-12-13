using Application.Common.Models;
using Application.Features.Products.Commands.CreateProduct;
using Application.Features.Products.Commands.DeleteProduct;
using Application.Features.Products.Commands.UpdateProduct;
using Application.Features.Products.Commands.ProductColors.AddProductColor;
using Application.Features.Products.Commands.ProductColors.UpdateProductColor;
using Application.Features.Products.Commands.ProductColors.DeleteProductColor;
using Application.Features.Products.Commands.ProductImages.AddProductImage;
using Application.Features.Products.Commands.ProductImages.DeleteProductImage;
using Application.Features.Products.Commands.ProductImages.ReorderProductImages;
using Application.Features.Products.DTOs;
using Application.Features.Products.Queries.GetProductById;
using Application.Features.Products.Queries.GetProductsList;
using Application.Features.Products.Queries.GetFeaturedProducts;
using Application.Features.Products.Queries.GetLatestProducts;
using Application.Features.Products.Queries.GetPopularProducts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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
        // Full create form: carries a JSON payload (CreateProductRequestDto) + all image files in one request
        public class CreateFullProductForm
        {
            /// <summary>
            /// JSON string representing CreateProductRequestDto (product + colors + images metadata).
            /// </summary>
            public string Payload { get; set; } = string.Empty;

            /// <summary>
            /// Optional main product image.
            /// </summary>
            public IFormFile? MainImageFile { get; set; }
        }

        public class UpdateProductForm
        {
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
            public decimal Price { get; set; }
            public bool IsActive { get; set; } = true;
            public int CategoryId { get; set; }
            public bool IsFeatured { get; set; } = false;
            public bool IsPopular { get; set; } = false;
            public bool IsLatest { get; set; } = false;
            public bool AllowAiCustomization { get; set; } = false;
            public string? AvailableSizes { get; set; }
            public IFormFile? ImageFile { get; set; }
        }

        // إنشاء منتج جديد في طلب واحد (المعلومات الأساسية + الألوان + الصور)
        [HttpPost]
        [ProducesResponseType(typeof(BaseResponse<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<int>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BaseResponse<int>>> Create([FromForm] CreateFullProductForm form, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(form.Payload))
            {
                return BadRequest(BaseResponse<int>.FailureResponse("Product payload is required"));
            }

            CreateProductRequestDto? dto;
            try
            {
                dto = JsonSerializer.Deserialize<CreateProductRequestDto>(form.Payload, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException ex)
            {
                return BadRequest(BaseResponse<int>.FailureResponse($"Invalid product payload format: {ex.Message}"));
            }

            if (dto == null)
            {
                return BadRequest(BaseResponse<int>.FailureResponse("Product payload is invalid"));
            }

            var cmd = new CreateProductCommand
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                IsActive = dto.IsActive,
                IsFeatured = dto.IsFeatured,
                IsPopular = dto.IsPopular,
                IsLatest = dto.IsLatest,
                CategoryId = dto.CategoryId,
                AvailableSizes = dto.AvailableSizes,
                ImageStream = form.MainImageFile?.OpenReadStream(),
                ImageFileName = form.MainImageFile?.FileName,
                Colors = dto.Colors
            };

            // Map all additional image files using their field name as FileKey
            foreach (var file in Request.Form.Files)
            {
                if (string.Equals(file.Name, nameof(CreateFullProductForm.MainImageFile), StringComparison.OrdinalIgnoreCase))
                {
                    continue; // main image already mapped
                }

                cmd.ImageStreams[file.Name] = file.OpenReadStream();
                cmd.ImageFileNames[file.Name] = file.FileName;
            }

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
                IsFeatured = form.IsFeatured,
                IsPopular = form.IsPopular,
                IsLatest = form.IsLatest,
                AllowAiCustomization = form.AllowAiCustomization,
                AvailableSizes = form.AvailableSizes,
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

        // إرجاع منتج واحد حسب المعرّف مع تفاصيل الألوان والصور
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(BaseResponse<ProductDetailsDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<BaseResponse<ProductDetailsDto>>> GetById([FromRoute] int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetProductByIdQuery { Id = id }, ct);
            return Ok(result);
        }

        // إرجاع قائمة منتجات مع دعم البحث والفرز والتقسيم إلى صفحات
        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<List<ProductListDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<BaseResponse<List<ProductListDto>>>> GetList([FromQuery] GetProductsListQuery query, CancellationToken ct)
        {
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }

        [HttpGet("featured")]
        [ProducesResponseType(typeof(BaseResponse<List<ProductListDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<BaseResponse<List<ProductListDto>>>> GetFeatured([FromQuery] GetFeaturedProductsQuery query, CancellationToken ct)
        {
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }

        [HttpGet("latest")]
        [ProducesResponseType(typeof(BaseResponse<List<ProductListDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<BaseResponse<List<ProductListDto>>>> GetLatest([FromQuery] GetLatestProductsQuery query, CancellationToken ct)
        {
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }

        [HttpGet("popular")]
        [ProducesResponseType(typeof(BaseResponse<List<ProductListDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<BaseResponse<List<ProductListDto>>>> GetPopular([FromQuery] GetPopularProductsQuery query, CancellationToken ct)
        {
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }

        // Product colors

        [HttpPost("{productId:int}/colors")]
        [ProducesResponseType(typeof(BaseResponse<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<int>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BaseResponse<int>>> AddColor([FromRoute] int productId, [FromBody] AddProductColorCommand command, CancellationToken ct)
        {
            command.ProductId = productId;
            var result = await _mediator.Send(command, ct);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("colors/{colorId:int}")]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BaseResponse<bool>>> UpdateColor([FromRoute] int colorId, [FromBody] UpdateProductColorCommand command, CancellationToken ct)
        {
            command.Id = colorId;
            var result = await _mediator.Send(command, ct);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("colors/{colorId:int}")]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BaseResponse<bool>>> DeleteColor([FromRoute] int colorId, CancellationToken ct)
        {
            var result = await _mediator.Send(new DeleteProductColorCommand { Id = colorId }, ct);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        // Product images

        public class AddProductImageForm
        {
            public string? AltText { get; set; }
            public bool? IsMain { get; set; }
            public IFormFile? ImageFile { get; set; }
        }

        [HttpPost("colors/{colorId:int}/images")]
        [ProducesResponseType(typeof(BaseResponse<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<int>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BaseResponse<int>>> AddImage([FromRoute] int colorId, [FromForm] AddProductImageForm form, CancellationToken ct)
        {
            if (form.ImageFile == null)
            {
                return BadRequest(BaseResponse<int>.FailureResponse("Image file is required"));
            }

            var cmd = new AddProductImageCommand
            {
                ProductColorId = colorId,
                AltText = form.AltText,
                IsMain = form.IsMain,
                ImageStream = form.ImageFile.OpenReadStream(),
                ImageFileName = form.ImageFile.FileName
            };

            var result = await _mediator.Send(cmd, ct);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("images/{imageId:int}")]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BaseResponse<bool>>> DeleteImage([FromRoute] int imageId, CancellationToken ct)
        {
            var result = await _mediator.Send(new DeleteProductImageCommand { Id = imageId }, ct);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("colors/{colorId:int}/images/reorder")]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BaseResponse<bool>>> ReorderImages([FromRoute] int colorId, [FromBody] ReorderProductImagesCommand command, CancellationToken ct)
        {
            command.ProductColorId = colorId;
            var result = await _mediator.Send(command, ct);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
    }
}
