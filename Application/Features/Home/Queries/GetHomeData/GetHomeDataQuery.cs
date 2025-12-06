using Application.Common.Models;
using Application.Features.Home.DTOs;
using MediatR;

namespace Application.Features.Home.Queries.GetHomeData
{
    public class GetHomeDataQuery : IRequest<BaseResponse<HomeDataDto>>
    {
    }
}
