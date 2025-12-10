using Application.Common.Models;
using Application.Features.TryOn.DTOs;
using MediatR;
using System.IO;

namespace Application.Features.TryOn.Commands.StartPipelineTryOn
{
    public class StartPipelineTryOnCommand : IRequest<BaseResponse<TryOnJobCreatedDto>>
    {
        public string UserId { get; set; } = string.Empty;
        public int CustomizationJobId { get; set; }
        public Stream PersonStream { get; set; } = Stream.Null;
        public string PersonFileName { get; set; } = string.Empty;
    }
}
