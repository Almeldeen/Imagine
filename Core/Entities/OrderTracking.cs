using System;

namespace Core.Entities
{
    public class OrderTracking : BaseEntity
    {
        public int OrderId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Comment { get; set; }
        public string? AdminUserId { get; set; }
        public string? AdminUserName { get; set; }

        public Order Order { get; set; } = null!;
    }
}
