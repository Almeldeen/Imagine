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
        private readonly ITryOnService _tryOnService;

        public GetTryOnStatusQueryHandler(ITryOnService tryOnService)
        {
            _tryOnService = tryOnService;
        }

        public Task<BaseResponse<TryOnJobStatusDto>> Handle(GetTryOnStatusQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.JobId))
            {
                return Task.FromResult(BaseResponse<TryOnJobStatusDto>.FailureResponse("Job id is required."));
            }

            return _tryOnService.GetTryOnStatusAsync(request.JobId, cancellationToken);
        }
    }
}
