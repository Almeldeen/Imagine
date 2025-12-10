using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.TryOn.DTOs;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.TryOn.Queries.GetTryOnStatus
{
    public class GetTryOnStatusQueryHandler : IRequestHandler<GetTryOnStatusQuery, BaseResponse<TryOnJobStatusDto>>
    {
        private readonly ITryOnPipelineService _pipelineService;

        public GetTryOnStatusQueryHandler(ITryOnPipelineService pipelineService)
        {
            _pipelineService = pipelineService;
        }

        public async Task<BaseResponse<TryOnJobStatusDto>> Handle(GetTryOnStatusQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.JobId))
            {
                return BaseResponse<TryOnJobStatusDto>.FailureResponse("Job id is required.");
            }

            var status = await _pipelineService.GetTryOnStatusAsync(request.JobId, cancellationToken);
            return BaseResponse<TryOnJobStatusDto>.SuccessResponse(status, "Try-on job status retrieved successfully.");
        }
    }
}
