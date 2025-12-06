using System;
using System.Collections.Generic;

namespace Application.Features.Orders.DTOs
{
    public class AdminOrderDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string? PaymentMethod { get; set; }

        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string? UserPhoneNumber { get; set; }

        public List<AdminOrderItemDto> Items { get; set; } = new();
    }
}
