using System.Collections.Generic;

namespace Core.Entities
{
    public class Product : BaseEntity
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal BasePrice { get; set; }
        public string? MainImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
        public bool IsPopular { get; set; } = false;
        public bool IsLatest { get; set; } = false;
        public int ViewCount { get; set; } = 0;

        public string? AvailableSizes { get; set; }

        public bool AllowAiCustomization { get; set; } = false;

        // Navigation Properties
        public Category Category { get; set; } = null!;
        public ICollection<ProductColor> Colors { get; set; } = new List<ProductColor>();
        public ICollection<UserColorSuggestion> ColorSuggestions { get; set; } = new List<UserColorSuggestion>();
        public ICollection<CustomProduct> CustomProducts { get; set; } = new List<CustomProduct>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
    }
}
