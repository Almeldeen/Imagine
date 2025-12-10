using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.TryOn.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class TryOnService : ITryOnService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TryOnService> _logger;
        private readonly string _apiKey;
        private readonly string _deApiKey;

        public TryOnService(HttpClient httpClient, IConfiguration configuration, ILogger<TryOnService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            var baseUrl = _configuration["TryOn:BaseUrl"] ?? "https://tryon-api.com";
            if (Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
            {
                _httpClient.BaseAddress = baseUri;
            }

            _apiKey = _configuration["TryOn:ApiKey"] ?? _configuration["TRYON_API_KEY"] ?? string.Empty;
            _deApiKey =
                _configuration["DeApi:ApiKey"] ??
                _configuration["DEAPI_API_KEY"] ??
                string.Empty;

            if (!string.IsNullOrWhiteSpace(_apiKey) && _httpClient.DefaultRequestHeaders.Authorization == null)
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            }

            if (_httpClient.DefaultRequestHeaders.Accept.Count == 0)
            {
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
        }

        public async Task<BaseResponse<PreprocessResultDto>> PreprocessGarmentAsync(Stream garmentStream, string fileName, string prompt, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_deApiKey))
            {
                _logger.LogError("DeApi API key is not configured. Set DeApi:ApiKey or DEAPI_API_KEY in configuration.");
                return BaseResponse<PreprocessResultDto>.FailureResponse("Image editing service is not configured. Please contact support.");
            }

            if (garmentStream == null || garmentStream == Stream.Null || string.IsNullOrWhiteSpace(fileName))
            {
                return BaseResponse<PreprocessResultDto>.FailureResponse("Garment image is required.");
            }

            if (string.IsNullOrWhiteSpace(prompt))
            {
                return BaseResponse<PreprocessResultDto>.FailureResponse("Prompt is required.");
            }

            byte[] buffer;
            using (var ms = new MemoryStream())
            {
                await garmentStream.CopyToAsync(ms, cancellationToken);
                buffer = ms.ToArray();
            }

            var deApiBase = _configuration["DeApi:BaseUrl"] ?? "https://api.deapi.ai";
            var model = _configuration["DeApi:Model"] ?? "QwenImageEdit_Plus_NF4";
            var steps = _configuration.GetValue<int?>("DeApi:Steps") ?? 20;
            var seed = _configuration.GetValue<int?>("DeApi:Seed") ?? 42;

            var trimmedBase = deApiBase.TrimEnd('/');
            var requestUri = $"{trimmedBase}/api/v1/client/img2img";

            return await SendWithRetryAsync(
                requestFactory: () =>
                {
                    var content = new MultipartFormDataContent();

                    content.Add(new StringContent(prompt), "prompt");
                    content.Add(new StringContent(model), "model");
                    content.Add(new StringContent(steps.ToString()), "steps");
                    content.Add(new StringContent(seed.ToString()), "seed");

                    var fileContent = new ByteArrayContent(buffer);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    content.Add(fileContent, "image", fileName);

                    var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
                    {
                        Content = content
                    };

                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _deApiKey);

                    return request;
                },
                operationName: "PreprocessGarment",
                successMessage: "Garment preprocessed successfully.",
                cancellationToken: cancellationToken,
                mapSuccess: async response =>
                {
                    var json = await response.Content.ReadAsStringAsync(cancellationToken);

                    if (string.IsNullOrWhiteSpace(json))
                    {
                        return new PreprocessResultDto();
                    }

                    try
                    {
                        using var doc = JsonDocument.Parse(json);
                        var root = doc.RootElement;

                        // Try common locations / property names for the resulting image.
                        string? url = null;

                        url = TryGetString(root,
                            "preprocessedImageUrl",
                            "imageUrl",
                            "image_url",
                            "resultImageUrl",
                            "result_image_url",
                            "url");

                        if (url == null && root.ValueKind == JsonValueKind.Object && root.TryGetProperty("data", out var dataElement))
                        {
                            url = TryGetString(dataElement,
                                "preprocessedImageUrl",
                                "imageUrl",
                                "image_url",
                                "resultImageUrl",
                                "result_image_url",
                                "url");
                        }

                        return new PreprocessResultDto
                        {
                            PreprocessedImageUrl = url ?? string.Empty
                        };
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse DeApi img2img response JSON.");
                        return new PreprocessResultDto();
                    }
                });
        }

        public async Task<BaseResponse<TryOnJobCreatedDto>> StartTryOnAsync(
            Stream personStream,
            string personFileName,
            Stream garmentStream,
            string garmentFileName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                _logger.LogError("TryOn API key is not configured. Set TryOn:ApiKey or TRYON_API_KEY in configuration.");
                return BaseResponse<TryOnJobCreatedDto>.FailureResponse("Try-on service is not configured. Please contact support.");
            }

            if (personStream == null || personStream == Stream.Null || string.IsNullOrWhiteSpace(personFileName))
            {
                return BaseResponse<TryOnJobCreatedDto>.FailureResponse("Person image is required.");
            }

            if (garmentStream == null || garmentStream == Stream.Null || string.IsNullOrWhiteSpace(garmentFileName))
            {
                return BaseResponse<TryOnJobCreatedDto>.FailureResponse("Garment image is required.");
            }

            byte[] personBuffer;
            byte[] garmentBuffer;

            using (var ms = new MemoryStream())
            {
                await personStream.CopyToAsync(ms, cancellationToken);
                personBuffer = ms.ToArray();
            }

            using (var ms = new MemoryStream())
            {
                await garmentStream.CopyToAsync(ms, cancellationToken);
                garmentBuffer = ms.ToArray();
            }

            return await SendWithRetryAsync(
                requestFactory: () =>
                {
                    var content = new MultipartFormDataContent();

                    var personContent = new ByteArrayContent(personBuffer);
                    personContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    content.Add(personContent, "person_images", personFileName);

                    var garmentContent = new ByteArrayContent(garmentBuffer);
                    garmentContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    content.Add(garmentContent, "garment_images", garmentFileName);

                    var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/tryon")
                    {
                        Content = content
                    };

                    return request;
                },
                operationName: "StartTryOn",
                successMessage: "Try-on job started successfully.",
                cancellationToken: cancellationToken,
                mapSuccess: async response =>
                {
                    var json = await response.Content.ReadAsStringAsync(cancellationToken);

                    if (string.IsNullOrWhiteSpace(json))
                    {
                        return new TryOnJobCreatedDto();
                    }

                    try
                    {
                        using var doc = JsonDocument.Parse(json);
                        var root = doc.RootElement;

                        var jobId = TryGetString(root, "jobId", "job_id", "id") ?? string.Empty;
                        var statusUrl = TryGetString(root, "statusUrl", "status_url");

                        return new TryOnJobCreatedDto
                        {
                            JobId = jobId,
                            StatusUrl = statusUrl
                        };
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse TryOn start-tryon response JSON.");
                        return new TryOnJobCreatedDto();
                    }
                });
        }

        public async Task<BaseResponse<TryOnJobStatusDto>> GetTryOnStatusAsync(string jobId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                _logger.LogError("TryOn API key is not configured. Set TryOn:ApiKey or TRYON_API_KEY in configuration.");
                return BaseResponse<TryOnJobStatusDto>.FailureResponse("Try-on service is not configured. Please contact support.");
            }

            if (string.IsNullOrWhiteSpace(jobId))
            {
                return BaseResponse<TryOnJobStatusDto>.FailureResponse("Job id is required.");
            }

            return await SendWithRetryAsync(
                requestFactory: () => new HttpRequestMessage(HttpMethod.Get, $"/api/v1/tryon/status/{jobId}"),
                operationName: "GetTryOnStatus",
                successMessage: "Try-on job status retrieved successfully.",
                cancellationToken: cancellationToken,
                mapSuccess: async response =>
                {
                    var json = await response.Content.ReadAsStringAsync(cancellationToken);

                    if (string.IsNullOrWhiteSpace(json))
                    {
                        return new TryOnJobStatusDto();
                    }

                    try
                    {
                        using var doc = JsonDocument.Parse(json);
                        var root = doc.RootElement;

                        var status = TryGetString(root, "status") ?? string.Empty;
                        var imageUrl = TryGetString(root, "imageUrl", "image_url");
                        var imageBase64 = TryGetString(root, "imageBase64", "image_base64");
                        var message = TryGetString(root, "message");
                        var error = TryGetString(root, "error");
                        var errorCode = TryGetString(root, "errorCode", "error_code");
                        var provider = TryGetString(root, "provider");

                        return new TryOnJobStatusDto
                        {
                            Status = status,
                            ImageUrl = imageUrl,
                            ImageBase64 = imageBase64,
                            Message = message,
                            Error = error,
                            ErrorCode = errorCode,
                            Provider = provider
                        };
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse TryOn status response JSON.");
                        return new TryOnJobStatusDto();
                    }
                });
        }

        private async Task<BaseResponse<T>> SendWithRetryAsync<T>(
            Func<HttpRequestMessage> requestFactory,
            string operationName,
            string successMessage,
            CancellationToken cancellationToken,
            Func<HttpResponseMessage, Task<T>> mapSuccess)
        {
            const int maxRetries = 2;

            for (var attempt = 0; attempt <= maxRetries; attempt++)
            {
                try
                {
                    using var request = requestFactory();
                    using var response = await _httpClient.SendAsync(request, cancellationToken);

                    if (response.IsSuccessStatusCode)
                    {
                        var data = await mapSuccess(response);
                        return BaseResponse<T>.SuccessResponse(data, successMessage);
                    }

                    if (IsTransientStatus(response.StatusCode) && attempt < maxRetries)
                    {
                        var delayMs = 500 * (int)Math.Pow(2, attempt);
                        _logger.LogWarning(
                            "Transient error during {Operation}. StatusCode={StatusCode}. Retrying in {Delay}ms (attempt {Attempt}/{TotalAttempts}).",
                            operationName,
                            (int)response.StatusCode,
                            delayMs,
                            attempt + 1,
                            maxRetries + 1);

                        await Task.Delay(delayMs, cancellationToken);
                        continue;
                    }

                    return await CreateFailureResponse<T>(operationName, response, cancellationToken);
                }
                catch (HttpRequestException ex) when (attempt < maxRetries)
                {
                    var delayMs = 500 * (int)Math.Pow(2, attempt);
                    _logger.LogWarning(ex,
                        "HTTP request error during {Operation}. Retrying in {Delay}ms (attempt {Attempt}/{TotalAttempts}).",
                        operationName,
                        delayMs,
                        attempt + 1,
                        maxRetries + 1);
                    await Task.Delay(delayMs, cancellationToken);
                }
                catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested && attempt < maxRetries)
                {
                    var delayMs = 500 * (int)Math.Pow(2, attempt);
                    _logger.LogWarning(ex,
                        "HTTP request timeout during {Operation}. Retrying in {Delay}ms (attempt {Attempt}/{TotalAttempts}).",
                        operationName,
                        delayMs,
                        attempt + 1,
                        maxRetries + 1);
                    await Task.Delay(delayMs, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error during {Operation} to TryOn API.", operationName);
                    return BaseResponse<T>.FailureResponse("Unexpected error while communicating with the try-on service. Please try again later.");
                }
            }

            return BaseResponse<T>.FailureResponse("Unable to reach the try-on service after multiple attempts. Please try again later.");
        }

        private static bool IsTransientStatus(HttpStatusCode statusCode)
        {
            var code = (int)statusCode;
            return code == 429 || code >= 500;
        }

        private async Task<BaseResponse<T>> CreateFailureResponse<T>(
            string operationName,
            HttpResponseMessage response,
            CancellationToken cancellationToken)
        {
            var statusCode = (int)response.StatusCode;
            string? body = null;

            try
            {
                body = await response.Content.ReadAsStringAsync(cancellationToken);
            }
            catch
            {
                // ignore body read errors
            }

            _logger.LogWarning(
                "TryOn API call {Operation} failed with status code {StatusCode}. Body: {Body}",
                operationName,
                statusCode,
                string.IsNullOrWhiteSpace(body) ? "<empty>" : Truncate(body, 500));

            string message = statusCode switch
            {
                400 => "The try-on service rejected the request. Please verify the images and try again.",
                401 or 403 => "Authentication with the try-on service failed. Please contact support.",
                402 => "Your try-on credits appear to be exhausted. Please try again later.",
                404 => "The requested try-on job could not be found.",
                429 => "The try-on service is receiving too many requests. Please wait a few seconds and try again.",
                _ => "The try-on service is currently unavailable. Please try again in a moment."
            };

            return BaseResponse<T>.FailureResponse(message);
        }

        private static string? TryGetString(JsonElement element, params string[] propertyNames)
        {
            foreach (var name in propertyNames)
            {
                if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
                {
                    return value.GetString();
                }

                // support different casing by scanning properties
                if (element.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in element.EnumerateObject())
                    {
                        if (string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase) && prop.Value.ValueKind == JsonValueKind.String)
                        {
                            return prop.Value.GetString();
                        }
                    }
                }
            }

            return null;
        }

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            {
                return value;
            }

            return value.Substring(0, maxLength) + "...";
        }
    }
}
