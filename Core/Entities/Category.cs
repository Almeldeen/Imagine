using System.Collections.Generic;

namespace Core.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;

        // Navigation Properties
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
