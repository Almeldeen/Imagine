namespace Application.Features.Products.DTOs
{
    public class ProductDto
    {
        // Unique identifier of the product
        public int Id { get; set; }

        // Product name (required)
        public string Name { get; set; } = string.Empty;

        // Optional product description
        public string? Description { get; set; }

        // Exposed price in the API (maps to Product.BasePrice in domain)
        public decimal Price { get; set; }

        // Whether the product is active/visible
        public bool IsActive { get; set; }

        // Main image URL exposed to clients (maps to Product.MainImageUrl)
        public string? ImageUrl { get; set; }

        // Auditing info propagated from BaseEntity
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
