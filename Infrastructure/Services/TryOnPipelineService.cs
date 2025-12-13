using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Application.Features.TryOn.DTOs;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Infrastructure.Services
{
    public class TryOnPipelineService : ITryOnPipelineService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITryOnService _tryOnService;
        private readonly ICustomizationJobRepository _customizationJobRepository;
        private readonly IImageService _imageService;
        private readonly ThirdPartyOptions _options;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TryOnPipelineService> _logger;

        public TryOnPipelineService(
            IHttpClientFactory httpClientFactory,
            ITryOnService tryOnService,
            ICustomizationJobRepository customizationJobRepository,
            IImageService imageService,
            IOptions<ThirdPartyOptions> options,
            IConfiguration configuration,
            ILogger<TryOnPipelineService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _tryOnService = tryOnService;
            _customizationJobRepository = customizationJobRepository;
            _imageService = imageService;
            _options = options.Value;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<(int CustomizationJobId, string DesignImageUrl)> GenerateDesignFromPromptAsync(
            string userId,
            string prompt,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User id is required.", nameof(userId));
            }

            if (string.IsNullOrWhiteSpace(prompt))
            {
                throw new ArgumentException("Prompt is required.", nameof(prompt));
            }

            var deApiKey = ResolveDeApiKey();
            if (string.IsNullOrWhiteSpace(deApiKey))
            {
                throw new InvalidOperationException("Image editing service is not configured.");
            }

            var job = new CustomizationJob
            {
                UserId = userId,
                Prompt = prompt,
                Status = CustomizationJobStatus.PendingGeneration
            };

            job = await _customizationJobRepository.AddAsync(job, cancellationToken);

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", deApiKey);

                var deApiBase = ResolveDeApiBase();
                var txt2ImgUrl = $"{deApiBase.TrimEnd('/')}/client/txt2img";

                var designPrompt = BuildDesignPrompt(prompt);
                string requestId = await SendDeApiTxt2ImgAsync(
                    client,
                    txt2ImgUrl,
                    designPrompt,
                    job.Id,
                    cancellationToken);

                job.DeApiRequestId = requestId;
                await _customizationJobRepository.UpdateAsync(job, cancellationToken);

                var statusUrlBase = $"{deApiBase.TrimEnd('/')}/client/request-status";
                var (resultUrl, finalStatus) = await PollDeApiStatusAsync(
                    client,
                    statusUrlBase,
                    requestId,
                    job.Id,
                    cancellationToken);

                if (!string.Equals(finalStatus, "done", StringComparison.OrdinalIgnoreCase))
                {
                    job.Status = CustomizationJobStatus.Failed;
                    job.LastError = "Design generation failed.";
                    await _customizationJobRepository.UpdateAsync(job, cancellationToken);
                    throw new InvalidOperationException(job.LastError);
                }

                var designBytes = await DownloadImageAsync(resultUrl, job.Id, cancellationToken);
                await using var ms = new MemoryStream(designBytes);
                ms.Position = 0;

                var upload = await _imageService.UploadImageAsync(ms, $"design-{job.Id}.png", folder: "designs", cancellationToken);
                if (!upload.Success || string.IsNullOrWhiteSpace(upload.Data))
                {
                    job.Status = CustomizationJobStatus.Failed;
                    job.LastError = string.IsNullOrWhiteSpace(upload.Message) ? "Failed to store generated design image." : upload.Message;
                    await _customizationJobRepository.UpdateAsync(job, cancellationToken);
                    throw new InvalidOperationException(job.LastError);
                }

                job.DesignImageUrl = upload.Data;
                job.Status = CustomizationJobStatus.DesignGenerated;
                job.LastError = null;
                await _customizationJobRepository.UpdateAsync(job, cancellationToken);

                return (job.Id, job.DesignImageUrl);
            }
            catch (Exception ex)
            {
                try
                {
                    job.Status = CustomizationJobStatus.Failed;
                    job.LastError = ex.Message;
                    await _customizationJobRepository.UpdateAsync(job, cancellationToken);
                }
                catch
                {
                    // ignored
                }

                throw;
            }
        }

        public async Task<(int CustomizationJobId, string FinalProductImageUrl)> ApplyDesignToGarmentAsync(
            string userId,
            int customizationJobId,
            Stream garmentStream,
            string garmentFileName,
            string? applyPrompt = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User id is required.", nameof(userId));
            }

            if (customizationJobId <= 0)
            {
                throw new ArgumentException("Customization job id is required.", nameof(customizationJobId));
            }

            if (garmentStream == null || garmentStream == Stream.Null || string.IsNullOrWhiteSpace(garmentFileName))
            {
                throw new ArgumentException("Garment image is required.", nameof(garmentStream));
            }

            var job = await _customizationJobRepository.GetByIdAsync(customizationJobId, cancellationToken);
            if (job == null || job.UserId != userId)
            {
                throw new InvalidOperationException("Customization job could not be found for this user.");
            }

            if (string.IsNullOrWhiteSpace(job.DesignImageUrl))
            {
                throw new InvalidOperationException("No design image is associated with this customization job.");
            }

            var deApiKey = ResolveDeApiKey();
            if (string.IsNullOrWhiteSpace(deApiKey))
            {
                throw new InvalidOperationException("Image editing service is not configured.");
            }

            byte[] garmentBytes;
            using (var ms = new MemoryStream())
            {
                await garmentStream.CopyToAsync(ms, cancellationToken);
                garmentBytes = ms.ToArray();
            }

            var designBytes = await LoadImageBytesAsync(job.DesignImageUrl!, customizationJobId, cancellationToken);
            var baseWithDesignBytes = ComposeDesignOntoGarment(garmentBytes, designBytes);

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", deApiKey);

            var deApiBase = ResolveDeApiBase();
            var img2ImgUrl = $"{deApiBase.TrimEnd('/')}/client/img2img";

            var effectiveApplyPrompt = string.IsNullOrWhiteSpace(applyPrompt)
                ? BuildApplyPrompt(job.Prompt)
                : applyPrompt!;

            string requestId = await SendDeApiImg2ImgAsync(
                client,
                img2ImgUrl,
                effectiveApplyPrompt,
                baseWithDesignBytes,
                $"customization-{customizationJobId}-base-with-design.png",
                customizationJobId,
                cancellationToken);

            job.DeApiRequestId = requestId;
            await _customizationJobRepository.UpdateAsync(job, cancellationToken);

            var statusUrlBase = $"{deApiBase.TrimEnd('/')}/client/request-status";
            var (resultUrl, finalStatus) = await PollDeApiStatusAsync(
                client,
                statusUrlBase,
                requestId,
                customizationJobId,
                cancellationToken);

            var finalUrl = resultUrl;

            try
            {
                var generatedBytes = await DownloadImageAsync(resultUrl, customizationJobId, cancellationToken);
                var compositedUrl = await TryComposePrintOntoOriginalAsync(
                    garmentBytes,
                    generatedBytes,
                    customizationJobId,
                    cancellationToken);

                if (!string.IsNullOrWhiteSpace(compositedUrl))
                {
                    finalUrl = compositedUrl;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to post-process final product image for customization job {CustomizationJobId}.", customizationJobId);
            }

            job.FinalProductImageUrl = finalUrl;
            job.GeneratedGarmentUrl = finalUrl;
            job.Status = string.Equals(finalStatus, "done", StringComparison.OrdinalIgnoreCase)
                ? CustomizationJobStatus.ProductImageGenerated
                : CustomizationJobStatus.Failed;
            job.LastError = job.Status == CustomizationJobStatus.Failed ? "Failed to apply design to garment." : null;
            await _customizationJobRepository.UpdateAsync(job, cancellationToken);

            return (job.Id, finalUrl);
        }

        public async Task<GenerateGarmentResultDto> GenerateGarmentFromPromptAsync(
            string userId,
            string prompt,
            Stream garmentStream,
            string garmentFileName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User id is required.", nameof(userId));
            }

            if (string.IsNullOrWhiteSpace(prompt))
            {
                throw new ArgumentException("Prompt is required.", nameof(prompt));
            }

            if (garmentStream == null || garmentStream == Stream.Null || string.IsNullOrWhiteSpace(garmentFileName))
            {
                throw new ArgumentException("Garment image is required.", nameof(garmentStream));
            }

            var deApiKey = ResolveDeApiKey();
            if (string.IsNullOrWhiteSpace(deApiKey))
            {
                throw new InvalidOperationException("Image editing service is not configured.");
            }

            byte[] garmentBytes;
            using (var ms = new MemoryStream())
            {
                await garmentStream.CopyToAsync(ms, cancellationToken);
                garmentBytes = ms.ToArray();
            }

            // Create job record
            var job = new CustomizationJob
            {
                UserId = userId,
                Prompt = prompt,
                Status = CustomizationJobStatus.PendingGeneration
            };

            job = await _customizationJobRepository.AddAsync(job, cancellationToken);

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", deApiKey);

            var deApiBase = ResolveDeApiBase();
            var img2ImgUrl = $"{deApiBase.TrimEnd('/')}/client/img2img";

            // 1) Start img2img job
            string requestId = await SendDeApiImg2ImgAsync(
                client,
                img2ImgUrl,
                prompt,
                garmentBytes,
                garmentFileName,
                job.Id,
                cancellationToken);

            job.DeApiRequestId = requestId;
            await _customizationJobRepository.UpdateAsync(job, cancellationToken);

            // 2) Poll request-status until done
            var statusUrlBase = $"{deApiBase.TrimEnd('/')}/client/request-status";
            var (resultUrl, finalStatus) = await PollDeApiStatusAsync(
                client,
                statusUrlBase,
                requestId,
                job.Id,
                cancellationToken);

            job.GeneratedGarmentUrl = resultUrl;
            job.Status = finalStatus == "done" ? CustomizationJobStatus.GarmentGenerated : CustomizationJobStatus.Failed;
            try
            {
                var generatedBytes = await DownloadImageAsync(resultUrl, job.Id, cancellationToken);
                var compositedUrl = await TryComposePrintOntoOriginalAsync(
                    garmentBytes,
                    generatedBytes,
                    customizationJobId: job.Id,
                    cancellationToken);

                if (!string.IsNullOrWhiteSpace(compositedUrl))
                {
                    job.GeneratedGarmentUrl = compositedUrl;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to post-process generated garment for customization job {CustomizationJobId}.", job.Id);
            }

            await _customizationJobRepository.UpdateAsync(job, cancellationToken);

            job.FinalProductImageUrl = job.GeneratedGarmentUrl;
            if (job.Status == CustomizationJobStatus.GarmentGenerated)
            {
                job.Status = CustomizationJobStatus.ProductImageGenerated;
            }
            await _customizationJobRepository.UpdateAsync(job, cancellationToken);

            return new GenerateGarmentResultDto
            {
                CustomizationJobId = job.Id,
                DeApiRequestId = requestId,
                GeneratedGarmentUrl = job.GeneratedGarmentUrl
            };
        }

        private static string BuildDesignPrompt(string prompt)
        {
            return (prompt.Trim() +
                    " High quality graphic design, centered composition, transparent background, print-ready." +
                    " No hoodie, no t-shirt, no human, no clothing, no model, no mannequin, no background.")
                .Trim();
        }

        private static string BuildApplyPrompt(string originalPrompt)
        {
            var core = string.IsNullOrWhiteSpace(originalPrompt) ? "the design" : originalPrompt.Trim();
            return ("Apply the provided design onto the garment in the center chest area. " +
                    "Keep the base garment color and background unchanged. " +
                    "Enhance realism: fabric texture, print shading, correct perspective. " +
                    "Do not add extra elements. " +
                    "Design description: " + core)
                .Trim();
        }

        private async Task<string> SendDeApiTxt2ImgAsync(
            HttpClient client,
            string url,
            string prompt,
            int customizationJobId,
            CancellationToken cancellationToken)
        {
            var model = _configuration["DeApi:Txt2ImgModel"] ?? "Flux1schnell";
            var configuredSteps = _configuration.GetValue<int?>("DeApi:Txt2ImgSteps") ?? 25;
            var steps = configuredSteps;
            if (steps > 10)
            {
                _logger.LogWarning(
                    "DEAPI txt2img steps value {Steps} is greater than the provider limit (10). Clamping to 10 for customization job {CustomizationJobId}.",
                    steps,
                    customizationJobId);
                steps = 10;
            }
            if (steps <= 0)
            {
                _logger.LogWarning(
                    "DEAPI txt2img steps value {Steps} is invalid. Falling back to 10 for customization job {CustomizationJobId}.",
                    configuredSteps,
                    customizationJobId);
                steps = 10;
            }
            var seed = _configuration.GetValue<int?>("DeApi:Seed") ?? 42;
            var width = _configuration.GetValue<int?>("DeApi:Txt2ImgWidth") ?? 1024;
            var height = _configuration.GetValue<int?>("DeApi:Txt2ImgHeight") ?? 1024;
            var guidance = _configuration.GetValue<double?>("DeApi:Txt2ImgGuidance") ?? 7.5;
            var negative = _configuration["DeApi:Txt2ImgNegativePrompt"] ?? "hoodie, t-shirt, clothing, model, mannequin, background";

            _logger.LogInformation(
                "Sending DEAPI txt2img request for customization job {CustomizationJobId}. Model={Model} Size={Width}x{Height} Steps={Steps} Guidance={Guidance} Seed={Seed}.",
                customizationJobId,
                model,
                width,
                height,
                steps,
                guidance,
                seed);

            async Task<string> SendWithResolutionAsync(int w, int h)
            {
                return await SendWithRetryAsync(
                    client,
                    () =>
                    {
                        var payload = new
                        {
                            prompt,
                            negative_prompt = negative,
                            model,
                            width = w,
                            height = h,
                            guidance,
                            steps,
                            seed
                        };

                        var json = JsonSerializer.Serialize(payload);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        return new HttpRequestMessage(HttpMethod.Post, url)
                        {
                            Content = content
                        };
                    },
                    "DeApi txt2img",
                    customizationJobId,
                    async response =>
                    {
                        var json = await response.Content.ReadAsStringAsync(cancellationToken);
                        if (string.IsNullOrWhiteSpace(json))
                        {
                            throw new InvalidOperationException("Empty response from DEAPI txt2img.");
                        }

                        using var doc = JsonDocument.Parse(json);
                        var root = doc.RootElement;

                        if (!root.TryGetProperty("data", out var dataElement))
                        {
                            throw new InvalidOperationException("DEAPI txt2img response did not contain 'data'.");
                        }

                        var requestId = dataElement.GetProperty("request_id").GetString();
                        if (string.IsNullOrWhiteSpace(requestId))
                        {
                            throw new InvalidOperationException("DEAPI txt2img response did not contain 'request_id'.");
                        }

                        _logger.LogInformation(
                            "Started DEAPI txt2img request {RequestId} for customization job {CustomizationJobId}.",
                            requestId,
                            customizationJobId);

                        return requestId!;
                    },
                    cancellationToken);
            }

            try
            {
                return await SendWithResolutionAsync(width, height);
            }
            catch (InvalidOperationException ex) when (
                ex.Message.Contains("status code 422", StringComparison.OrdinalIgnoreCase) &&
                (width != 512 || height != 512))
            {
                _logger.LogWarning(
                    ex,
                    "DEAPI txt2img returned 422 for customization job {CustomizationJobId}. Retrying once with 512x512 resolution.",
                    customizationJobId);

                return await SendWithResolutionAsync(512, 512);
            }
        }

        private async Task<byte[]> LoadImageBytesAsync(string imageUrl, int customizationJobId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                throw new ArgumentException("Image URL is required.", nameof(imageUrl));
            }

            var pathPart = imageUrl;
            if (Uri.TryCreate(imageUrl, UriKind.Absolute, out var absUri))
            {
                pathPart = absUri.AbsolutePath;
            }

            if (!string.IsNullOrWhiteSpace(pathPart) && pathPart.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
            {
                var resolvedPath = _imageService.GetImagePath(pathPart);
                if (!string.IsNullOrWhiteSpace(resolvedPath) && System.IO.File.Exists(resolvedPath))
                {
                    return await System.IO.File.ReadAllBytesAsync(resolvedPath, cancellationToken);
                }
            }

            var effectiveUrl = imageUrl;
            if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out _))
            {
                var baseUrl = _configuration["App:BaseUrl"] ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(baseUrl) && imageUrl.StartsWith("/"))
                {
                    effectiveUrl = $"{baseUrl.TrimEnd('/')}{imageUrl}";
                }
            }

            return await DownloadImageAsync(effectiveUrl, customizationJobId, cancellationToken);
        }

        private static byte[] ComposeDesignOntoGarment(byte[] garmentBytes, byte[] designBytes)
        {
            using var garment = Image.Load<Rgba32>(garmentBytes);
            using var design = Image.Load<Rgba32>(designBytes);

            var rect = new Rectangle(
                (int)(garment.Width * 0.22f),
                (int)(garment.Height * 0.28f),
                (int)(garment.Width * 0.56f),
                (int)(garment.Height * 0.44f));

            rect = ClampRect(rect, garment.Width, garment.Height);
            if (rect.Width <= 0 || rect.Height <= 0)
            {
                return garmentBytes;
            }

            var maxW = Math.Max(1, (int)(rect.Width * 0.9f));
            var maxH = Math.Max(1, (int)(rect.Height * 0.9f));
            var scale = Math.Min(maxW / (float)design.Width, maxH / (float)design.Height);
            scale = Math.Min(scale, 1f);

            var targetW = Math.Max(1, (int)(design.Width * scale));
            var targetH = Math.Max(1, (int)(design.Height * scale));

            using var resized = design.Clone(ctx => ctx.Resize(targetW, targetH));

            var x = rect.X + (rect.Width - targetW) / 2;
            var y = rect.Y + (rect.Height - targetH) / 2;

            garment.Mutate(ctx => ctx.DrawImage(resized, new Point(x, y), 1f));

            using var outMs = new MemoryStream();
            garment.SaveAsPng(outMs);
            return outMs.ToArray();
        }

        private async Task<string?> TryComposePrintOntoOriginalAsync(
            byte[] originalGarmentBytes,
            byte[] generatedGarmentBytes,
            int customizationJobId,
            CancellationToken cancellationToken)
        {
            if (originalGarmentBytes == null || originalGarmentBytes.Length == 0 ||
                generatedGarmentBytes == null || generatedGarmentBytes.Length == 0)
            {
                return null;
            }

            using var original = Image.Load<Rgba32>(originalGarmentBytes);
            using var generated = Image.Load<Rgba32>(generatedGarmentBytes);

            if (original.Width != generated.Width || original.Height != generated.Height)
            {
                generated.Mutate(x => x.Resize(original.Width, original.Height));
            }
            var rect = new Rectangle(
                (int)(original.Width * 0.22f),
                (int)(original.Height * 0.28f),
                (int)(original.Width * 0.56f),
                (int)(original.Height * 0.44f));

            rect = ClampRect(rect, original.Width, original.Height);
            if (rect.Width <= 0 || rect.Height <= 0)
            {
                return null;
            }

            using var crop = generated.Clone(ctx => ctx.Crop(rect));
            var bg = EstimateBackgroundColor(crop);
            ApplyChromaAlphaMask(crop, bg);

            original.Mutate(ctx => ctx.DrawImage(crop, new Point(rect.X, rect.Y), 1f));

            await using var ms = new MemoryStream();
            await original.SaveAsPngAsync(ms, cancellationToken);
            ms.Position = 0;

            var fileName = $"customization-{customizationJobId}-composited.png";
            var upload = await _imageService.UploadImageAsync(ms, fileName, folder: "customizations", cancellationToken);
            return upload.Success ? upload.Data : null;
        }

        private static Rectangle ClampRect(Rectangle rect, int width, int height)
        {
            var x = Math.Clamp(rect.X, 0, Math.Max(0, width - 1));
            var y = Math.Clamp(rect.Y, 0, Math.Max(0, height - 1));
            var w = Math.Clamp(rect.Width, 0, width - x);
            var h = Math.Clamp(rect.Height, 0, height - y);
            return new Rectangle(x, y, w, h);
        }

        private static Rgba32 EstimateBackgroundColor(Image<Rgba32> crop)
        {
            var w = crop.Width;
            var h = crop.Height;
            var margin = Math.Max(2, (int)(Math.Min(w, h) * 0.06f));

            var samples = new (int X, int Y)[]
            {
                (margin, margin),
                (w - margin - 1, margin),
                (margin, h - margin - 1),
                (w - margin - 1, h - margin - 1),
                (w / 2, margin),
                (w / 2, h - margin - 1)
            };

            long r = 0, g = 0, b = 0;
            var count = 0;

            foreach (var (x, y) in samples)
            {
                if (x < 0 || x >= w || y < 0 || y >= h) continue;
                var px = crop[x, y];
                r += px.R;
                g += px.G;
                b += px.B;
                count++;
            }

            if (count == 0)
            {
                return new Rgba32(0, 0, 0);
            }

            return new Rgba32((byte)(r / count), (byte)(g / count), (byte)(b / count));
        }

        private static void ApplyChromaAlphaMask(Image<Rgba32> crop, Rgba32 bg)
        {
            const float t0 = 18f;
            const float t1 = 55f;

            crop.ProcessPixelRows(accessor =>
            {
                for (var y = 0; y < crop.Height; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (var x = 0; x < row.Length; x++)
                    {
                        var px = row[x];
                        var dr = px.R - bg.R;
                        var dg = px.G - bg.G;
                        var db = px.B - bg.B;
                        var dist = MathF.Sqrt(dr * dr + dg * dg + db * db);

                        byte a;
                        if (dist <= t0)
                        {
                            a = 0;
                        }
                        else if (dist >= t1)
                        {
                            a = 255;
                        }
                        else
                        {
                            a = (byte)(Math.Clamp((dist - t0) / (t1 - t0), 0f, 1f) * 255f);
                        }

                        px.A = a;
                        row[x] = px;
                    }
                }
            });
        }

        public async Task<TryOnJobCreatedDto> StartTryOnAsync(
            string userId,
            int customizationJobId,
            Stream personStream,
            string personFileName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User id is required.", nameof(userId));
            }

            if (personStream == null || personStream == Stream.Null || string.IsNullOrWhiteSpace(personFileName))
            {
                throw new ArgumentException("Person image is required.", nameof(personStream));
            }

            var job = await _customizationJobRepository.GetByIdAsync(customizationJobId, cancellationToken);
            if (job == null || job.UserId != userId)
            {
                throw new InvalidOperationException("Customization job could not be found for this user.");
            }

            var garmentUrl = job.FinalProductImageUrl ?? job.GeneratedGarmentUrl;
            if (string.IsNullOrWhiteSpace(garmentUrl))
            {
                throw new InvalidOperationException("No generated garment is associated with this customization job.");
            }

            // Download the generated garment image
            var garmentBytes = await LoadImageBytesAsync(garmentUrl, job.Id, cancellationToken);

            byte[] personBytes;
            using (var ms = new MemoryStream())
            {
                await personStream.CopyToAsync(ms, cancellationToken);
                personBytes = ms.ToArray();
            }

            // Call existing TryOnService to start the job
            using var personMs = new MemoryStream(personBytes);
            using var garmentMs = new MemoryStream(garmentBytes);

            var tryOnResponse = await _tryOnService.StartTryOnAsync(
                personMs,
                personFileName,
                garmentMs,
                DeriveFileNameFromUrl(garmentUrl) ?? "generated-garment.png",
                cancellationToken);

            if (!tryOnResponse.Success || tryOnResponse.Data == null)
            {
                job.Status = CustomizationJobStatus.Failed;
                job.LastError = string.IsNullOrWhiteSpace(tryOnResponse.Message)
                    ? "Failed to start try-on job."
                    : tryOnResponse.Message;
                await _customizationJobRepository.UpdateAsync(job, cancellationToken);

                throw new InvalidOperationException(job.LastError);
            }

            job.TryOnJobId = tryOnResponse.Data.JobId;
            job.TryOnStatusUrl = tryOnResponse.Data.StatusUrl;
            job.Status = CustomizationJobStatus.TryOnStarted;
            job.LastError = null;
            await _customizationJobRepository.UpdateAsync(job, cancellationToken);

            _logger.LogInformation(
                "Started TryOn job {JobId} for customization job {CustomizationJobId}.",
                job.TryOnJobId,
                job.Id);

            return tryOnResponse.Data;
        }

        public async Task<TryOnJobStatusDto> GetTryOnStatusAsync(
            string jobId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(jobId))
            {
                throw new ArgumentException("Job id is required.", nameof(jobId));
            }

            var response = await _tryOnService.GetTryOnStatusAsync(jobId, cancellationToken);

            if (!response.Success || response.Data == null)
            {
                throw new InvalidOperationException(
                    string.IsNullOrWhiteSpace(response.Message)
                        ? "Failed to get try-on status."
                        : response.Message);
            }

            var statusDto = response.Data;

            // Optionally update CustomizationJob when completed/failed
            try
            {
                var job = await FindJobByTryOnJobIdAsync(jobId, cancellationToken);
                if (job != null)
                {
                    var status = (statusDto.Status ?? string.Empty).ToLowerInvariant();

                    if (status == "completed" || status == "done")
                    {
                        job.Status = CustomizationJobStatus.Completed;
                        job.TryOnResultUrl = statusDto.ImageUrl ?? job.TryOnResultUrl;
                        job.LastError = null;
                        await _customizationJobRepository.UpdateAsync(job, cancellationToken);
                    }
                    else if (status == "failed")
                    {
                        job.Status = CustomizationJobStatus.Failed;
                        job.LastError = statusDto.Error ?? statusDto.Message;
                        await _customizationJobRepository.UpdateAsync(job, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update customization job for TryOn job {JobId}.", jobId);
            }

            return statusDto;
        }

        private string ResolveDeApiKey()
        {
            // Prefer explicit DeApi:ApiKey from configuration (with optional DEAPI_API_KEY env fallback)
            var keyFromConfig = _configuration["DeApi:ApiKey"] ?? _configuration["DEAPI_API_KEY"];

            return string.IsNullOrWhiteSpace(keyFromConfig) ? string.Empty : keyFromConfig;
        }

        private string ResolveDeApiBase()
        {
            return !string.IsNullOrWhiteSpace(_options.DeApiBase)
                ? _options.DeApiBase
                : _configuration["DeApi:BaseUrl"] ?? "https://api.deapi.ai/api/v1";
        }

        private async Task<string> SendDeApiImg2ImgAsync(
            HttpClient client,
            string url,
            string prompt,
            byte[] garmentBytes,
            string garmentFileName,
            int customizationJobId,
            CancellationToken cancellationToken)
        {
            var model = _configuration["DeApi:Model"] ?? "QwenImageEdit_Plus_NF4";
            var steps = _configuration.GetValue<int?>("DeApi:Steps") ?? 20;
            var seed = _configuration.GetValue<int?>("DeApi:Seed") ?? 42;

            return await SendWithRetryAsync(
                client,
                () =>
                {
                    var content = new MultipartFormDataContent();
                    content.Add(new StringContent(prompt), "prompt");
                    content.Add(new StringContent(model), "model");
                    content.Add(new StringContent(steps.ToString()), "steps");
                    content.Add(new StringContent(seed.ToString()), "seed");

                    var fileContent = new ByteArrayContent(garmentBytes);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    content.Add(fileContent, "image", garmentFileName);

                    return new HttpRequestMessage(HttpMethod.Post, url)
                    {
                        Content = content
                    };
                },
                "DeApi img2img",
                customizationJobId,
                async response =>
                {
                    var json = await response.Content.ReadAsStringAsync(cancellationToken);
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        throw new InvalidOperationException("Empty response from DEAPI img2img.");
                    }

                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    if (!root.TryGetProperty("data", out var dataElement))
                    {
                        throw new InvalidOperationException("DEAPI img2img response did not contain 'data'.");
                    }

                    var requestId = dataElement.GetProperty("request_id").GetString();
                    if (string.IsNullOrWhiteSpace(requestId))
                    {
                        throw new InvalidOperationException("DEAPI img2img response did not contain 'request_id'.");
                    }

                    _logger.LogInformation(
                        "Started DEAPI img2img request {RequestId} for customization job {CustomizationJobId}.",
                        requestId,
                        customizationJobId);

                    return requestId!;
                },
                cancellationToken);
        }

        private async Task<(string ResultUrl, string FinalStatus)> PollDeApiStatusAsync(
            HttpClient client,
            string statusBaseUrl,
            string requestId,
            int customizationJobId,
            CancellationToken cancellationToken)
        {
            var start = DateTime.UtcNow;
            var maxSeconds = 120;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var elapsed = (DateTime.UtcNow - start).TotalSeconds;
                if (elapsed > maxSeconds)
                {
                    throw new TimeoutException("DEAPI request is taking longer than expected.");
                }

                var url = $"{statusBaseUrl.TrimEnd('/')}/{requestId}";

                var (status, resultUrl) = await SendWithRetryAsync(
                    client,
                    () => new HttpRequestMessage(HttpMethod.Get, url),
                    "DeApi request-status",
                    customizationJobId,
                    async response =>
                    {
                        var json = await response.Content.ReadAsStringAsync(cancellationToken);
                        if (string.IsNullOrWhiteSpace(json))
                        {
                            throw new InvalidOperationException("Empty response from DEAPI request-status.");
                        }

                        using var doc = JsonDocument.Parse(json);
                        var root = doc.RootElement;

                        if (!root.TryGetProperty("data", out var dataElement))
                        {
                            throw new InvalidOperationException("DEAPI request-status response did not contain 'data'.");
                        }

                        var status = dataElement.GetProperty("status").GetString() ?? string.Empty;
                        var resultUrl = dataElement.TryGetProperty("result_url", out var resultElement)
                            ? resultElement.GetString()
                            : null;

                        return (status, resultUrl ?? string.Empty);
                    },
                    cancellationToken);

                var normalizedStatus = (status ?? string.Empty).ToLowerInvariant();

                if (normalizedStatus == "done")
                {
                    if (string.IsNullOrWhiteSpace(resultUrl))
                    {
                        throw new InvalidOperationException("DEAPI reported 'done' but no result_url was provided.");
                    }

                    _logger.LogInformation(
                        "DEAPI request {RequestId} for customization job {CustomizationJobId} completed.",
                        requestId,
                        customizationJobId);

                    return (resultUrl, normalizedStatus);
                }

                if (normalizedStatus == "failed")
                {
                    throw new InvalidOperationException("DEAPI reported the request as failed.");
                }

                // queued / processing
                var delayMs = elapsed < 10 ? 2000 : 5000;
                await Task.Delay(delayMs, cancellationToken);
            }
        }

        private async Task<byte[]> DownloadImageAsync(string url, int customizationJobId, CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient();

            return await SendWithRetryAsync(
                client,
                () => new HttpRequestMessage(HttpMethod.Get, url),
                "Download generated garment",
                customizationJobId,
                async response => await response.Content.ReadAsByteArrayAsync(cancellationToken),
                cancellationToken);
        }

        private async Task<CustomizationJob?> FindJobByTryOnJobIdAsync(string tryOnJobId, CancellationToken cancellationToken)
        {
            // Simple scan; consider optimizing with a dedicated query if needed
            var queryable = _customizationJobRepository.GetAllQueryable();
            return await Task.Run(() => queryable.FirstOrDefault(j => j.TryOnJobId == tryOnJobId), cancellationToken);
        }

        private static string? DeriveFileNameFromUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            try
            {
                var uri = new Uri(url);
                var name = Path.GetFileName(uri.LocalPath);
                return string.IsNullOrWhiteSpace(name) ? null : name;
            }
            catch
            {
                return null;
            }
        }

        private async Task<T> SendWithRetryAsync<T>(
            HttpClient client,
            Func<HttpRequestMessage> requestFactory,
            string operationName,
            int customizationJobId,
            Func<HttpResponseMessage, Task<T>> mapSuccess,
            CancellationToken cancellationToken)
        {
            const int maxRetries = 2;

            for (var attempt = 0; attempt <= maxRetries; attempt++)
            {
                try
                {
                    using var request = requestFactory();
                    using var response = await client.SendAsync(request, cancellationToken);

                    if (response.IsSuccessStatusCode)
                    {
                        return await mapSuccess(response);
                    }

                    if (IsTransientStatus(response.StatusCode) && attempt < maxRetries)
                    {
                        var delayMs = 500 * (int)Math.Pow(2, attempt);
                        _logger.LogWarning(
                            "Transient error during {Operation} for customization job {CustomizationJobId}. StatusCode={StatusCode}. Retrying in {Delay}ms (attempt {Attempt}/{TotalAttempts}).",
                            operationName,
                            customizationJobId,
                            (int)response.StatusCode,
                            delayMs,
                            attempt + 1,
                            maxRetries + 1);

                        await Task.Delay(delayMs, cancellationToken);
                        continue;
                    }

                    var statusCode = (int)response.StatusCode;
                    string? responseBody = null;
                    try
                    {
                        responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    }
                    catch
                    {
                        // ignored
                    }

                    if (!string.IsNullOrWhiteSpace(responseBody) && responseBody.Length > 4000)
                    {
                        responseBody = responseBody.Substring(0, 4000);
                    }

                    _logger.LogError(
                        "{Operation} failed for customization job {CustomizationJobId}. StatusCode={StatusCode}. ResponseBody={ResponseBody}",
                        operationName,
                        customizationJobId,
                        statusCode,
                        responseBody);

                    var message = string.IsNullOrWhiteSpace(responseBody)
                        ? $"{operationName} failed with status code {statusCode}."
                        : $"{operationName} failed with status code {statusCode}. Response: {responseBody}";

                    throw new InvalidOperationException(message);
                }
                catch (HttpRequestException ex) when (attempt < maxRetries)
                {
                    var delayMs = 500 * (int)Math.Pow(2, attempt);
                    _logger.LogWarning(ex,
                        "HTTP error during {Operation} for customization job {CustomizationJobId}. Retrying in {Delay}ms (attempt {Attempt}/{TotalAttempts}).",
                        operationName,
                        customizationJobId,
                        delayMs,
                        attempt + 1,
                        maxRetries + 1);
                    await Task.Delay(delayMs, cancellationToken);
                }
                catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested && attempt < maxRetries)
                {
                    var delayMs = 500 * (int)Math.Pow(2, attempt);
                    _logger.LogWarning(ex,
                        "Timeout during {Operation} for customization job {CustomizationJobId}. Retrying in {Delay}ms (attempt {Attempt}/{TotalAttempts}).",
                        operationName,
                        customizationJobId,
                        delayMs,
                        attempt + 1,
                        maxRetries + 1);
                    await Task.Delay(delayMs, cancellationToken);
                }
            }

            throw new InvalidOperationException($"Unable to complete {operationName} after multiple attempts.");
        }

        private static bool IsTransientStatus(HttpStatusCode statusCode)
        {
            var code = (int)statusCode;
            return code == 429 || code >= 500;
        }
    }
}
