namespace Application.Features.Products.DTOs
{
    /// <summary>
    /// DTO representing a single product image for a specific color.
    /// </summary>
    public class ProductImageDto
    {
        public int Id { get; set; }
        public int ProductColorId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? AltText { get; set; }
        public bool IsMain { get; set; }
        public int DisplayOrder { get; set; }
    }
}
