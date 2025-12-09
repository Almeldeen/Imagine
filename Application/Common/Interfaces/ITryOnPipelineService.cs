using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.TryOn.DTOs;

namespace Application.Common.Interfaces
{
    public interface ITryOnPipelineService
    {
        Task<GenerateGarmentResultDto> GenerateGarmentFromPromptAsync(
            string userId,
            string prompt,
            Stream garmentStream,
            string garmentFileName,
            CancellationToken cancellationToken = default);

        Task<TryOnJobCreatedDto> StartTryOnAsync(
            string userId,
            Stream personStream,
            string personFileName,
            string generatedGarmentUrl,
            CancellationToken cancellationToken = default);

        Task<TryOnJobStatusDto> GetTryOnStatusAsync(
            string jobId,
            CancellationToken cancellationToken = default);
    }
}
