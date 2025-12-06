using System;

namespace Application.Features.Orders.DTOs
{
    public class AdminOrderItemDto
    {
        public string ProductName { get; set; } = string.Empty;
        public string? ColorName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string? ProductImageUrl { get; set; }
    }
}
