using Application.Common.Models;
using Application.Features.TryOn.DTOs;
using MediatR;
using System.IO;

namespace Application.Features.TryOn.Commands.StartTryOn
{
    public class StartTryOnCommand : IRequest<BaseResponse<TryOnJobCreatedDto>>
    {
        public Stream PersonStream { get; set; } = Stream.Null;
        public string PersonFileName { get; set; } = string.Empty;
        public Stream GarmentStream { get; set; } = Stream.Null;
        public string GarmentFileName { get; set; } = string.Empty;
    }
}
