using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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

namespace Infrastructure.Services
{
    public class TryOnPipelineService : ITryOnPipelineService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITryOnService _tryOnService;
        private readonly ICustomizationJobRepository _customizationJobRepository;
        private readonly ThirdPartyOptions _options;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TryOnPipelineService> _logger;

        public TryOnPipelineService(
            IHttpClientFactory httpClientFactory,
            ITryOnService tryOnService,
            ICustomizationJobRepository customizationJobRepository,
            IOptions<ThirdPartyOptions> options,
            IConfiguration configuration,
            ILogger<TryOnPipelineService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _tryOnService = tryOnService;
            _customizationJobRepository = customizationJobRepository;
            _options = options.Value;
            _configuration = configuration;
            _logger = logger;
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
            await _customizationJobRepository.UpdateAsync(job, cancellationToken);

            return new GenerateGarmentResultDto
            {
                CustomizationJobId = job.Id,
                DeApiRequestId = requestId,
                GeneratedGarmentUrl = resultUrl
            };
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

            if (string.IsNullOrWhiteSpace(job.GeneratedGarmentUrl))
            {
                throw new InvalidOperationException("No generated garment is associated with this customization job.");
            }

            // Download the generated garment image
            var garmentBytes = await DownloadImageAsync(job.GeneratedGarmentUrl, job.Id, cancellationToken);

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
                DeriveFileNameFromUrl(job.GeneratedGarmentUrl) ?? "generated-garment.png",
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
                    var message = $"{operationName} failed with status code {statusCode}.";
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
