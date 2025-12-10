using Application.Common.Models;
using Application.Features.AdminDashboard.DTOs;
using MediatR;

namespace Application.Features.AdminDashboard.Queries.GetDashboardStats
{
    public class GetDashboardStatsQuery : IRequest<BaseResponse<AdminDashboardStatsDto>>
    {
    }
}
