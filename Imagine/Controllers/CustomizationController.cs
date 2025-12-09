using Application.Common.Models;
using Application.Features.TryOn.Commands.PreprocessGarment;
using Application.Features.TryOn.Commands.StartTryOn;
using Application.Features.TryOn.DTOs;
using Application.Features.TryOn.Queries.GetTryOnStatus;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        public CustomizationController(IMediator mediator, IMemoryCache memoryCache, IConfiguration configuration)
        {
            _mediator = mediator;
            _memoryCache = memoryCache;
            _configuration = configuration;
        }

        public class StartTryOnForm
        {
            public IFormFile? PersonImage { get; set; }
            public IFormFile? GarmentImage { get; set; }
        }

        public class PreprocessGarmentForm
        {
            public IFormFile? File { get; set; }
            public string? Prompt { get; set; }
            public string? GarmentType { get; set; }
        }

        [HttpPost("preprocess")]
        [ProducesResponseType(typeof(BaseResponse<PreprocessResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<PreprocessResultDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<PreprocessResultDto>), StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<BaseResponse<PreprocessResultDto>>> PreprocessGarment([FromForm] PreprocessGarmentForm form, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest(BaseResponse<PreprocessResultDto>.FailureResponse("User id was not found in the access token."));
            }

            if (!CheckRateLimit(userId, out var rateLimitResult))
            {
                return rateLimitResult!;
            }

            if (string.IsNullOrWhiteSpace(form.Prompt))
            {
                return BadRequest(BaseResponse<PreprocessResultDto>.FailureResponse("Prompt is required."));
            }

            Stream garmentStream;
            string fileName;

            if (form.File != null && form.File.Length > 0)
            {
                var validationError = ValidateImageFile(form.File);
                if (validationError != null)
                {
                    return BadRequest(BaseResponse<PreprocessResultDto>.FailureResponse(validationError));
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
                    return BadRequest(BaseResponse<PreprocessResultDto>.FailureResponse(errorMessage ?? "Default garment image not found."));
                }

                garmentStream = resolved.Value.Stream;
                fileName = resolved.Value.FileName;
            }

            var command = new PreprocessGarmentCommand
            {
                GarmentStream = garmentStream,
                FileName = fileName,
                Prompt = form.Prompt!.Trim()
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
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

            if (form.GarmentImage == null || form.GarmentImage.Length == 0)
            {
                return BadRequest(BaseResponse<TryOnJobCreatedDto>.FailureResponse("Garment image file is required."));
            }

            var personError = ValidateImageFile(form.PersonImage);
            if (personError != null)
            {
                return BadRequest(BaseResponse<TryOnJobCreatedDto>.FailureResponse(personError));
            }

            var garmentError = ValidateImageFile(form.GarmentImage);
            if (garmentError != null)
            {
                return BadRequest(BaseResponse<TryOnJobCreatedDto>.FailureResponse(garmentError));
            }

            var command = new StartTryOnCommand
            {
                PersonStream = form.PersonImage.OpenReadStream(),
                PersonFileName = form.PersonImage.FileName,
                GarmentStream = form.GarmentImage.OpenReadStream(),
                GarmentFileName = form.GarmentImage.FileName
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

            if (normalized == "tshirt" || normalized == "t-shirt")
            {
                path = !string.IsNullOrWhiteSpace(tshirtPathFromConfig)
                    ? tshirtPathFromConfig
                    : Path.Combine(Directory.GetCurrentDirectory(), "ClientApp", "public", "assets", "images", "T-Shirt.png");
            }
            else
            {
                path = !string.IsNullOrWhiteSpace(hoodiePathFromConfig)
                    ? hoodiePathFromConfig
                    : Path.Combine(Directory.GetCurrentDirectory(), "ClientApp", "public", "assets", "images", "White Hoodie.png");
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
