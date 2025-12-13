using System;
using System.Collections.Generic;

namespace Application.Features.Products.DTOs
{
    /// <summary>
    /// Detailed DTO used when showing a full product with colors and images.
    /// </summary>
    public class ProductDetailsDto
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsPopular { get; set; }
        public bool IsLatest { get; set; }
        public int ViewCount { get; set; }
        public bool AllowAiCustomization { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string? AvailableSizes { get; set; }

        public List<ProductColorDto> Colors { get; set; } = new();
    }
}
