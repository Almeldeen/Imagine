namespace Core.Entities
{
    public class WishlistItem : BaseEntity
    {
        public int WishlistId { get; set; }
        public int ProductId { get; set; }
        public int? ProductColorId { get; set; }

        // Navigation Properties
        public Wishlist Wishlist { get; set; } = null!;
        public Product Product { get; set; } = null!;
        public ProductColor? ProductColor { get; set; }
    }
}
