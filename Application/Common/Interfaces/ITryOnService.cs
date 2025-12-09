using System.IO;
using System.Threading;
using Application.Common.Models;
using Application.Features.TryOn.DTOs;

namespace Application.Common.Interfaces
{
    public interface ITryOnService
    {
        Task<BaseResponse<PreprocessResultDto>> PreprocessGarmentAsync(
            Stream garmentStream,
            string fileName,
            string prompt,
            CancellationToken cancellationToken = default);

        Task<BaseResponse<TryOnJobCreatedDto>> StartTryOnAsync(
            Stream personStream,
            string personFileName,
            Stream garmentStream,
            string garmentFileName,
            CancellationToken cancellationToken = default);

        Task<BaseResponse<TryOnJobStatusDto>> GetTryOnStatusAsync(string jobId, CancellationToken cancellationToken = default);
    }
}
