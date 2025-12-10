using Application.Common.Models;
using Application.Features.AdminAnalytics.DTOs;
using MediatR;

namespace Application.Features.AdminAnalytics.Queries.GetSalesOverview
{
    public class GetSalesOverviewQuery : IRequest<BaseResponse<SalesOverviewDto>>
    {
        public string Period { get; set; } = "month"; // "month" (last 12 months) or "year" (last 5 years)
    }
}
