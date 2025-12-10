using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.TryOn.DTOs;
using MediatR;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.TryOn.Commands.GenerateGarment
{
    public class GenerateGarmentCommandHandler : IRequestHandler<GenerateGarmentCommand, BaseResponse<GenerateGarmentResultDto>>
    {
        private readonly ITryOnPipelineService _pipelineService;

        public GenerateGarmentCommandHandler(ITryOnPipelineService pipelineService)
        {
            _pipelineService = pipelineService;
        }

        public async Task<BaseResponse<GenerateGarmentResultDto>> Handle(GenerateGarmentCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                return BaseResponse<GenerateGarmentResultDto>.FailureResponse("User id is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BaseResponse<GenerateGarmentResultDto>.FailureResponse("Prompt is required.");
            }

            if (request.GarmentStream == null || request.GarmentStream == Stream.Null || string.IsNullOrWhiteSpace(request.FileName))
            {
                return BaseResponse<GenerateGarmentResultDto>.FailureResponse("Garment image is required.");
            }

            try
            {
                var result = await _pipelineService.GenerateGarmentFromPromptAsync(
                    request.UserId,
                    request.Prompt,
                    request.GarmentStream,
                    request.FileName,
                    cancellationToken);

                return BaseResponse<GenerateGarmentResultDto>.SuccessResponse(result, "Garment generated successfully.");
            }
            finally
            {
                request.GarmentStream?.Dispose();
            }
        }
    }
}
