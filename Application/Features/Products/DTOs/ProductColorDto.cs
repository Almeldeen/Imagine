using System.Collections.Generic;

namespace Application.Features.Products.DTOs
{
    /// <summary>
    /// DTO representing a product color variation with its images.
    /// </summary>
    public class ProductColorDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ColorName { get; set; } = string.Empty;
        public string? ColorHex { get; set; }
        public int Stock { get; set; }
        public decimal AdditionalPrice { get; set; }
        public bool IsAvailable { get; set; }

        public List<ProductImageDto> Images { get; set; } = new();
    }
}
