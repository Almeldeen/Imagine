using System.Collections.Generic;

namespace Application.Features.AdminAnalytics.DTOs
{
    public class SalesOverviewDto
    {
        public List<SalesDataPointDto> Points { get; set; } = new();
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
    }
}
