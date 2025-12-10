using Application.Common.Models;
using Application.Features.TryOn.Commands.GenerateGarment;
using Application.Features.TryOn.Commands.PreprocessGarment;
using Application.Features.TryOn.Commands.StartPipelineTryOn;
using Application.Features.TryOn.Commands.StartTryOn;
using Application.Features.TryOn.DTOs;
using Application.Features.TryOn.Queries.GetTryOnStatus;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Imagine.Controllers
{
    [ApiController]
    [Route("api/customization")]
    [Produces("application/json")]
    [Authorize]
    public class CustomizationController : ControllerBase
    {
        private static readonly string[] AllowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxImageSizeBytes = 8 * 1024 * 1024;

        private readonly IMediator _mediator;
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public CustomizationController(
            IMediator mediator,
            IMemoryCache memoryCache,
            IConfiguration configuration,
            IWebHostEnvironment env)
        {
            _mediator = mediator;
            _memoryCache = memoryCache;
            _configuration = configuration;
            _env = env;
        }

        public class StartTryOnForm
        {
            public IFormFile? PersonImage { get; set; }
            public int CustomizationJobId { get; set; }
        }

        public class PreprocessGarmentForm
        {
            public IFormFile? File { get; set; }
            public string? Prompt { get; set; }
            public string? GarmentType { get; set; }
        }

        [HttpPost("generate")]
        [ProducesResponseType(typeof(BaseResponse<GenerateGarmentResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<GenerateGarmentResultDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<GenerateGarmentResultDto>), StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<BaseResponse<GenerateGarmentResultDto>>> GenerateGarment([FromForm] PreprocessGarmentForm form, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest(BaseResponse<GenerateGarmentResultDto>.FailureResponse("User id was not found in the access token."));
            }

            if (!CheckRateLimit(userId, out var rateLimitResult))
            {
                return rateLimitResult!;
            }

            if (string.IsNullOrWhiteSpace(form.Prompt))
            {
                return BadRequest(BaseResponse<GenerateGarmentResultDto>.FailureResponse("Prompt is required."));
            }

            Stream garmentStream;
            string fileName;

            if (form.File != null && form.File.Length > 0)
            {
                var validationError = ValidateImageFile(form.File);
                if (validationError != null)
                {
                    return BadRequest(BaseResponse<GenerateGarmentResultDto>.FailureResponse(validationError));
                }

                garmentStream = form.File.OpenReadStream();
                fileName = form.File.FileName;
            }
            else
            {
                var garmentType = string.IsNullOrWhiteSpace(form.GarmentType)
                    ? "hoodie"
                    : form.GarmentType!.ToLowerInvariant();

                var resolved = ResolveDefaultGarmentImage(garmentType, out var errorMessage);
                if (resolved == null)
                {
                    return BadRequest(BaseResponse<GenerateGarmentResultDto>.FailureResponse(errorMessage ?? "Default garment image not found."));
                }

                garmentStream = resolved.Value.Stream;
                fileName = resolved.Value.FileName;
            }

            var command = new GenerateGarmentCommand
            {
                UserId = userId,
                Prompt = form.Prompt!.Trim(),
                GarmentStream = garmentStream,
                FileName = fileName
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("preprocess")]
        [ProducesResponseType(typeof(BaseResponse<PreprocessResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<PreprocessResultDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<PreprocessResultDto>), StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<BaseResponse<PreprocessResultDto>>> PreprocessGarment([FromForm] PreprocessGarmentForm form, CancellationToken cancellationToken)
        {
            var generateResult = await GenerateGarment(form, cancellationToken);

            BaseResponse<GenerateGarmentResultDto>? generateResponse = null;

            if (generateResult.Result is ObjectResult objectResult && objectResult.Value is BaseResponse<GenerateGarmentResultDto> objectData)
            {
                generateResponse = objectData;
            }
            else if (generateResult.Value is BaseResponse<GenerateGarmentResultDto> valueData)
            {
                generateResponse = valueData;
            }

            if (generateResponse == null)
            {
                return BadRequest(BaseResponse<PreprocessResultDto>.FailureResponse("Failed to preprocess garment."));
            }

            if (!generateResponse.Success || generateResponse.Data == null || string.IsNullOrWhiteSpace(generateResponse.Data.GeneratedGarmentUrl))
            {
                return BadRequest(BaseResponse<PreprocessResultDto>.FailureResponse(string.IsNullOrWhiteSpace(generateResponse.Message)
                    ? "Failed to preprocess garment."
                    : generateResponse.Message));
            }

            var preDto = new PreprocessResultDto
            {
                PreprocessedImageUrl = generateResponse.Data.GeneratedGarmentUrl!,
                CustomizationJobId = generateResponse.Data.CustomizationJobId
            };

            var mapped = BaseResponse<PreprocessResultDto>.SuccessResponse(preDto, generateResponse.Message);
            return Ok(mapped);
        }

        [HttpPost("tryon")]
        [ProducesResponseType(typeof(BaseResponse<TryOnJobCreatedDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<TryOnJobCreatedDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<TryOnJobCreatedDto>), StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<BaseResponse<TryOnJobCreatedDto>>> StartTryOn([FromForm] StartTryOnForm form, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest(BaseResponse<TryOnJobCreatedDto>.FailureResponse("User id was not found in the access token."));
            }

            if (!CheckRateLimit(userId, out var rateLimitResult))
            {
                return rateLimitResult!;
            }

            if (form.PersonImage == null || form.PersonImage.Length == 0)
            {
                return BadRequest(BaseResponse<TryOnJobCreatedDto>.FailureResponse("Person image file is required."));
            }

            if (form.CustomizationJobId <= 0)
            {
                return BadRequest(BaseResponse<TryOnJobCreatedDto>.FailureResponse("A valid customization job id is required."));
            }

            var personError = ValidateImageFile(form.PersonImage);
            if (personError != null)
            {
                return BadRequest(BaseResponse<TryOnJobCreatedDto>.FailureResponse(personError));
            }

            var command = new StartPipelineTryOnCommand
            {
                UserId = userId,
                CustomizationJobId = form.CustomizationJobId,
                PersonStream = form.PersonImage.OpenReadStream(),
                PersonFileName = form.PersonImage.FileName
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("tryon/{jobId}")]
        [ProducesResponseType(typeof(BaseResponse<TryOnJobStatusDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<TryOnJobStatusDto>), StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<BaseResponse<TryOnJobStatusDto>>> GetTryOnStatus(string jobId, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest(BaseResponse<TryOnJobStatusDto>.FailureResponse("User id was not found in the access token."));
            }

            if (!CheckRateLimit(userId, out var rateLimitResult))
            {
                return rateLimitResult!;
            }

            var query = new GetTryOnStatusQuery
            {
                JobId = jobId
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        private string? ValidateImageFile(IFormFile file)
        {
            if (file.Length > MaxImageSizeBytes)
            {
                return $"Image is too large. Maximum allowed size is {MaxImageSizeBytes / (1024 * 1024)} MB.";
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(extension))
            {
                return "Unsupported image type. Allowed types are: .jpg, .jpeg, .png, .webp.";
            }

            return null;
        }

        private string? GetUserId()
        {
            return User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                   User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private bool CheckRateLimit(string userId, out ActionResult? errorResult)
        {
            var maxPerMinute = _configuration.GetValue<int?>("TryOn:MaxRequestsPerMinute") ?? 10;
            var windowSeconds = _configuration.GetValue<int?>("TryOn:RateLimitWindowSeconds") ?? 60;

            var cacheKey = $"tryon:rate:{userId}";
            var now = DateTime.UtcNow;

            var entry = _memoryCache.Get<RateLimitEntry>(cacheKey);
            if (entry == null || (now - entry.WindowStartUtc).TotalSeconds > windowSeconds)
            {
                entry = new RateLimitEntry
                {
                    WindowStartUtc = now,
                    Count = 1
                };

                _memoryCache.Set(cacheKey, entry, TimeSpan.FromMinutes(5));
                errorResult = null;
                return true;
            }

            if (entry.Count >= maxPerMinute)
            {
                var response = BaseResponse<object>.FailureResponse("You are sending too many try-on requests. Please wait a moment and try again.");
                errorResult = StatusCode(StatusCodes.Status429TooManyRequests, response);
                return false;
            }

            entry.Count++;
            errorResult = null;
            return true;
        }

        private (Stream Stream, string FileName)? ResolveDefaultGarmentImage(string garmentType, out string? errorMessage)
        {
            var normalized = string.IsNullOrWhiteSpace(garmentType)
                ? "hoodie"
                : garmentType.ToLowerInvariant();

            var hoodiePathFromConfig = _configuration["TryOn:DefaultGarmentImages:Hoodie"];
            var tshirtPathFromConfig = _configuration["TryOn:DefaultGarmentImages:TShirt"];

            string path;
            var contentRoot = _env.ContentRootPath;

            if (normalized == "tshirt" || normalized == "t-shirt")
            {
                if (!string.IsNullOrWhiteSpace(tshirtPathFromConfig))
                {
                    path = tshirtPathFromConfig;
                }
                else
                {
                    var candidatePublic = Path.Combine(contentRoot, "ClientApp", "public", "assets", "images", "T-Shirt.png");
                    var candidateDist = Path.Combine(contentRoot, "ClientApp", "dist", "imagine.client", "browser", "assets", "images", "T-Shirt.png");

                    path = System.IO.File.Exists(candidatePublic) ? candidatePublic : candidateDist;
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(hoodiePathFromConfig))
                {
                    path = hoodiePathFromConfig;
                }
                else
                {
                    var candidatePublic = Path.Combine(contentRoot, "ClientApp", "public", "assets", "images", "White Hoodie.png");
                    var candidateDist = Path.Combine(contentRoot, "ClientApp", "dist", "imagine.client", "browser", "assets", "images", "White Hoodie.png");

                    path = System.IO.File.Exists(candidatePublic) ? candidatePublic : candidateDist;
                }
            }

            if (!System.IO.File.Exists(path))
            {
                errorMessage = $"Default {normalized} image was not found on the server.";
                return null;
            }

            errorMessage = null;
            var stream = System.IO.File.OpenRead(path);
            var fileName = Path.GetFileName(path);
            return (stream, fileName);
        }

        private class RateLimitEntry
        {
            public DateTime WindowStartUtc { get; set; }
            public int Count { get; set; }
        }
    }
}
