using System;

namespace Application.Features.AdminDashboard.DTOs
{
    public class AdminDashboardStatsDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int TotalProducts { get; set; }
        public int TotalCustomers { get; set; }
    }
}
