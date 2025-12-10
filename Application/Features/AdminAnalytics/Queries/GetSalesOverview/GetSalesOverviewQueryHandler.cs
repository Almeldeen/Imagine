using Application.Common.Models;
using Application.Features.AdminAnalytics.DTOs;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminAnalytics.Queries.GetSalesOverview
{
    public class GetSalesOverviewQueryHandler : IRequestHandler<GetSalesOverviewQuery, BaseResponse<SalesOverviewDto>>
    {
        private readonly IOrderRepository _orderRepository;

        public GetSalesOverviewQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<BaseResponse<SalesOverviewDto>> Handle(GetSalesOverviewQuery request, CancellationToken cancellationToken)
        {
            var ordersQuery = _orderRepository
                .GetAllQueryable()
                .Where(o => o.Status != OrderStatus.Cancelled && o.Status != OrderStatus.Refunded);

            var now = DateTime.UtcNow;

            if (string.Equals(request.Period, "year", StringComparison.OrdinalIgnoreCase))
            {
                var fromYear = now.Year - 4; // last 5 years including current

                // Let EF Core handle grouping and simple aggregates only
                var yearlyData = await ordersQuery
                    .Where(o => o.OrderDate.Year >= fromYear)
                    .GroupBy(o => o.OrderDate.Year)
                    .Select(g => new
                    {
                        Year = g.Key,
                        TotalRevenue = g.Sum(x => x.TotalAmount),
                        TotalOrders = g.Count()
                    })
                    .OrderBy(x => x.Year)
                    .ToListAsync(cancellationToken);

                // Map to DTOs in memory so we can safely format dates/strings
                var yearly = yearlyData
                    .Select(x =>
                    {
                        var periodStart = new DateTime(x.Year, 1, 1);
                        var average = x.TotalOrders == 0 ? 0 : x.TotalRevenue / x.TotalOrders;

                        return new SalesDataPointDto
                        {
                            Label = x.Year.ToString(),
                            PeriodStart = periodStart,
                            TotalRevenue = x.TotalRevenue,
                            TotalOrders = x.TotalOrders,
                            AverageOrderValue = average
                        };
                    })
                    .OrderBy(p => p.PeriodStart)
                    .ToList();

                var overviewYear = new SalesOverviewDto
                {
                    Points = yearly,
                    TotalRevenue = yearly.Sum(p => p.TotalRevenue),
                    TotalOrders = yearly.Sum(p => p.TotalOrders)
                };

                return BaseResponse<SalesOverviewDto>.SuccessResponse(overviewYear, "Yearly sales overview retrieved successfully");
            }
            else
            {
                // Default: monthly for last 12 months
                var fromDate = now.AddMonths(-11); // include current month => 12 months
                var monthStart = new DateTime(fromDate.Year, fromDate.Month, 1);

                // EF Core only groups and aggregates; mapping to DTO happens in memory
                var monthlyData = await ordersQuery
                    .Where(o => o.OrderDate >= monthStart)
                    .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                    .Select(g => new
                    {
                        g.Key.Year,
                        g.Key.Month,
                        TotalRevenue = g.Sum(x => x.TotalAmount),
                        TotalOrders = g.Count()
                    })
                    .OrderBy(x => x.Year)
                    .ThenBy(x => x.Month)
                    .ToListAsync(cancellationToken);

                var monthly = monthlyData
                    .Select(x =>
                    {
                        var periodStart = new DateTime(x.Year, x.Month, 1);
                        var average = x.TotalOrders == 0 ? 0 : x.TotalRevenue / x.TotalOrders;

                        return new SalesDataPointDto
                        {
                            Label = periodStart.ToString("MMM"),
                            PeriodStart = periodStart,
                            TotalRevenue = x.TotalRevenue,
                            TotalOrders = x.TotalOrders,
                            AverageOrderValue = average
                        };
                    })
                    .OrderBy(p => p.PeriodStart)
                    .ToList();

                var overviewMonth = new SalesOverviewDto
                {
                    Points = monthly,
                    TotalRevenue = monthly.Sum(p => p.TotalRevenue),
                    TotalOrders = monthly.Sum(p => p.TotalOrders)
                };

                return BaseResponse<SalesOverviewDto>.SuccessResponse(overviewMonth, "Monthly sales overview retrieved successfully");
            }
        }
    }
}
