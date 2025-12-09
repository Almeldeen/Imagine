using Application.Common.Models;
using Application.Features.TryOn.DTOs;
using MediatR;

namespace Application.Features.TryOn.Queries.GetTryOnStatus
{
    public class GetTryOnStatusQuery : IRequest<BaseResponse<TryOnJobStatusDto>>
    {
        public string JobId { get; set; } = string.Empty;
    }
}
