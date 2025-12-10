using Application.Common.Models;
using Application.Features.TryOn.DTOs;
using MediatR;
using System.IO;

namespace Application.Features.TryOn.Commands.GenerateGarment
{
    public class GenerateGarmentCommand : IRequest<BaseResponse<GenerateGarmentResultDto>>
    {
        public string UserId { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;
        public Stream GarmentStream { get; set; } = Stream.Null;
        public string FileName { get; set; } = string.Empty;
    }
}
