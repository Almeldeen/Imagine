using System;

namespace Application.Features.AdminAnalytics.DTOs
{
    public class SalesDataPointDto
    {
        public string Label { get; set; } = string.Empty; // e.g. "Jan", "2024", "2024-01-15"
        public DateTime PeriodStart { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
    }
}
