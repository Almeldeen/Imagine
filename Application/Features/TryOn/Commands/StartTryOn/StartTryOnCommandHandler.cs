using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.TryOn.DTOs;
using MediatR;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.TryOn.Commands.StartTryOn
{
    public class StartTryOnCommandHandler : IRequestHandler<StartTryOnCommand, BaseResponse<TryOnJobCreatedDto>>
    {
        private readonly ITryOnService _tryOnService;

        public StartTryOnCommandHandler(ITryOnService tryOnService)
        {
            _tryOnService = tryOnService;
        }

        public async Task<BaseResponse<TryOnJobCreatedDto>> Handle(StartTryOnCommand request, CancellationToken cancellationToken)
        {
            if (request.PersonStream == null || request.PersonStream == Stream.Null || string.IsNullOrWhiteSpace(request.PersonFileName))
            {
                return BaseResponse<TryOnJobCreatedDto>.FailureResponse("Person image is required.");
            }

            if (request.GarmentStream == null || request.GarmentStream == Stream.Null || string.IsNullOrWhiteSpace(request.GarmentFileName))
            {
                return BaseResponse<TryOnJobCreatedDto>.FailureResponse("Garment image is required.");
            }

            try
            {
                return await _tryOnService.StartTryOnAsync(
                    request.PersonStream,
                    request.PersonFileName,
                    request.GarmentStream,
                    request.GarmentFileName,
                    cancellationToken);
            }
            finally
            {
                request.PersonStream?.Dispose();
                request.GarmentStream?.Dispose();
            }
        }
    }
}
