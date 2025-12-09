using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.TryOn.DTOs;
using MediatR;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.TryOn.Commands.PreprocessGarment
{
    public class PreprocessGarmentCommandHandler : IRequestHandler<PreprocessGarmentCommand, BaseResponse<PreprocessResultDto>>
    {
        private readonly ITryOnService _tryOnService;

        public PreprocessGarmentCommandHandler(ITryOnService tryOnService)
        {
            _tryOnService = tryOnService;
        }

        public async Task<BaseResponse<PreprocessResultDto>> Handle(PreprocessGarmentCommand request, CancellationToken cancellationToken)
        {
            if (request.GarmentStream == null || request.GarmentStream == Stream.Null || string.IsNullOrWhiteSpace(request.FileName))
            {
                return BaseResponse<PreprocessResultDto>.FailureResponse("Garment image is required.");
            }

            try
            {
                return await _tryOnService.PreprocessGarmentAsync(request.GarmentStream, request.FileName, request.Prompt, cancellationToken);
            }
            finally
            {
                request.GarmentStream?.Dispose();
            }
        }
    }
}
