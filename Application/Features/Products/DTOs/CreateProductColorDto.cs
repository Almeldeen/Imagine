using System.Collections.Generic;

namespace Application.Features.Products.DTOs
{
    /// <summary>
    /// Input DTO for a product color variation when creating a product.
    /// </summary>
    public class CreateProductColorDto
    {
        public string ColorName { get; set; } = string.Empty;
        public string? ColorHex { get; set; }
        public int Stock { get; set; }
        public decimal AdditionalPrice { get; set; }
        public bool IsAvailable { get; set; } = true;

        public List<CreateProductImageDto> Images { get; set; } = new();
    }
}
