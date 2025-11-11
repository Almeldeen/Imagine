using Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Entities
{
    public class Order : BaseEntity
    {
        public string? UserId { get; set; }
        public string OrderNumber { get; set; } = null!;
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal SubTotal { get; set; }
        public decimal ShippingCost { get; set; } = 0m;
        public decimal Tax { get; set; } = 0m;
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        
        // Shipping Information
        public string ShippingAddress { get; set; } = null!;
        public string ShippingCity { get; set; } = null!;
        public string ShippingPostalCode { get; set; } = null!;
        public string ShippingCountry { get; set; } = null!;
        public string? ShippingPhone { get; set; }
        
        // Payment Information
        public string? PaymentMethod { get; set; }
        public string? PaymentTransactionId { get; set; }
        public DateTime? PaidAt { get; set; }
        
        // Tracking
        public string? TrackingNumber { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }

        // Navigation Properties
        public ApplicationUser? User { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
