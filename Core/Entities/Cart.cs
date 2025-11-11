using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Entities
{
    public class Cart : BaseEntity
    {
        public string? UserId { get; set; }
        public string? SessionId { get; set; }
        public DateTime? ExpiresAt { get; set; }

        // Navigation Properties
        public ApplicationUser? User { get; set; }
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();

        // Computed Properties
        public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
        public int TotalItems => Items.Sum(i => i.Quantity);
    }
}
