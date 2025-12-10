using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.TryOn.DTOs;
using MediatR;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.TryOn.Commands.StartPipelineTryOn
{
    public class StartPipelineTryOnCommandHandler : IRequestHandler<StartPipelineTryOnCommand, BaseResponse<TryOnJobCreatedDto>>
    {
        private readonly ITryOnPipelineService _pipelineService;

        public StartPipelineTryOnCommandHandler(ITryOnPipelineService pipelineService)
        {
            _pipelineService = pipelineService;
        }

        public async Task<BaseResponse<TryOnJobCreatedDto>> Handle(StartPipelineTryOnCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                return BaseResponse<TryOnJobCreatedDto>.FailureResponse("User id is required.");
            }

            if (request.CustomizationJobId <= 0)
            {
                return BaseResponse<TryOnJobCreatedDto>.FailureResponse("A valid customization job id is required.");
            }

            if (request.PersonStream == null || request.PersonStream == Stream.Null || string.IsNullOrWhiteSpace(request.PersonFileName))
            {
                return BaseResponse<TryOnJobCreatedDto>.FailureResponse("Person image is required.");
            }

            try
            {
                var result = await _pipelineService.StartTryOnAsync(
                    request.UserId,
                    request.CustomizationJobId,
                    request.PersonStream,
                    request.PersonFileName,
                    cancellationToken);

                return BaseResponse<TryOnJobCreatedDto>.SuccessResponse(result, "Try-on job started successfully.");
            }
            finally
            {
                request.PersonStream?.Dispose();
            }
        }
    }
}
