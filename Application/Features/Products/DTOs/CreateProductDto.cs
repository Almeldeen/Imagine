namespace Application.Features.Products.DTOs
{
    /// <summary>
    /// Simple input DTO used when creating a product from UI forms.
    /// Actual command may still carry Stream for image to keep Application decoupled from ASP.NET.
    /// </summary>
    public class CreateProductDto
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
    }
}
