using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Application.Features.TryOn.DTOs;
using Core.Interfaces;
using Infrastructure.Configuration;
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
        private readonly ILogger<TryOnPipelineService> _logger;

        public TryOnPipelineService(
            IHttpClientFactory httpClientFactory,
            ITryOnService tryOnService,
            ICustomizationJobRepository customizationJobRepository,
            IOptions<ThirdPartyOptions> options,
            ILogger<TryOnPipelineService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _tryOnService = tryOnService;
            _customizationJobRepository = customizationJobRepository;
            _options = options.Value;
            _logger = logger;
        }

        public Task<GenerateGarmentResultDto> GenerateGarmentFromPromptAsync(
            string userId,
            string prompt,
            Stream garmentStream,
            string garmentFileName,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<TryOnJobCreatedDto> StartTryOnAsync(
            string userId,
            Stream personStream,
            string personFileName,
            string generatedGarmentUrl,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<TryOnJobStatusDto> GetTryOnStatusAsync(
            string jobId,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
