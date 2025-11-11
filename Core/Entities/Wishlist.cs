using System.Collections.Generic;

namespace Core.Entities
{
    public class Wishlist : BaseEntity
    {
        public string UserId { get; set; } = null!;
        public string Name { get; set; } = "My Wishlist";
        public bool IsDefault { get; set; } = true;

        // Navigation Properties
        public ApplicationUser User { get; set; } = null!;
        public ICollection<WishlistItem> Items { get; set; } = new List<WishlistItem>();
    }
}
