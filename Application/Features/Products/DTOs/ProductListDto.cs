using System;
using System.Collections.Generic;

namespace Application.Features.Products.DTOs
{
    /// <summary>
    /// Lightweight DTO used for product list views (admin and public).
    /// Mirrors ProductDto but kept separate for clarity when needed.
    /// </summary>
    public class ProductListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public int ViewCount { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<ProductColorDto> Colors { get; set; } = new();
    }
}
