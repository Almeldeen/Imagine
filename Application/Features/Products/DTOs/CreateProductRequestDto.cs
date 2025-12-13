using System.Collections.Generic;

namespace Application.Features.Products.DTOs
{
    /// <summary>
    /// Full create request DTO carrying product basics plus nested colors and images.
    /// This is used at the API boundary (controller) then mapped into the command.
    /// </summary>
    public class CreateProductRequestDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
        public bool IsPopular { get; set; } = false;
        public bool IsLatest { get; set; } = false;

        public bool AllowAiCustomization { get; set; } = false;

        public string? AvailableSizes { get; set; }

        public List<CreateProductColorDto> Colors { get; set; } = new();
    }
}
