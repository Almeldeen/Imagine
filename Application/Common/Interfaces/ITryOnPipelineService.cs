using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.TryOn.DTOs;

namespace Application.Common.Interfaces
{
    public interface ITryOnPipelineService
    {
        Task<(int CustomizationJobId, string DesignImageUrl)> GenerateDesignFromPromptAsync(
            string userId,
            string prompt,
            CancellationToken cancellationToken = default);

        Task<(int CustomizationJobId, string FinalProductImageUrl)> ApplyDesignToGarmentAsync(
            string userId,
            int customizationJobId,
            Stream garmentStream,
            string garmentFileName,
            string? applyPrompt = null,
            CancellationToken cancellationToken = default);

        Task<GenerateGarmentResultDto> GenerateGarmentFromPromptAsync(
            string userId,
            string prompt,
            Stream garmentStream,
            string garmentFileName,
            CancellationToken cancellationToken = default);

        Task<TryOnJobCreatedDto> StartTryOnAsync(
            string userId,
            int customizationJobId,
            Stream personStream,
            string personFileName,
            CancellationToken cancellationToken = default);

        Task<TryOnJobStatusDto> GetTryOnStatusAsync(
            string jobId,
            CancellationToken cancellationToken = default);
    }
}
