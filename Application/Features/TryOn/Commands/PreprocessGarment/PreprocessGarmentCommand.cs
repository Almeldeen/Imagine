using Application.Common.Models;
using Application.Features.TryOn.DTOs;
using MediatR;
using System.IO;

namespace Application.Features.TryOn.Commands.PreprocessGarment
{
    public class PreprocessGarmentCommand : IRequest<BaseResponse<PreprocessResultDto>>
    {
        public Stream GarmentStream { get; set; } = Stream.Null;
        public string FileName { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;
    }
}
