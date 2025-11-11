using System.Collections.Generic;

namespace Core.Entities
{
    public class ProductColor : BaseEntity
    {
        public int ProductId { get; set; }
        public string ColorName { get; set; } = null!;
        public string? ColorHex { get; set; }
        public int Stock { get; set; } = 0;
        public decimal AdditionalPrice { get; set; } = 0m;
        public bool IsAvailable { get; set; } = true;

        // Navigation Properties
        public Product Product { get; set; } = null!;
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
    }
}
