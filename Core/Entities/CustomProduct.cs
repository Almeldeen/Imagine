using Core.Enums;
using System.Collections.Generic;

namespace Core.Entities
{
    public class CustomProduct : BaseEntity
    {
        public string? UserId { get; set; }
        public int? ProductId { get; set; }
        public string? CustomDesignImageUrl { get; set; }
        public string? AIRenderedPreviewUrl { get; set; }
        public string? Notes { get; set; }
        public decimal EstimatedPrice { get; set; }
        public CustomProductStatus Status { get; set; } = CustomProductStatus.Draft;
        public string? AdminNotes { get; set; }

        // Navigation Properties
        public ApplicationUser? User { get; set; }
        public Product? Product { get; set; }
        public ICollection<CustomProductColor> CustomColors { get; set; } = new List<CustomProductColor>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
