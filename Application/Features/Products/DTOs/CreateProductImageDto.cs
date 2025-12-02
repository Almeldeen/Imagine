namespace Application.Features.Products.DTOs
{
    /// <summary>
    /// Input DTO for a product image when creating a product.
    /// File content is supplied separately via multipart/form-data and mapped using FileKey.
    /// </summary>
    public class CreateProductImageDto
    {
        public string FileKey { get; set; } = string.Empty;
        public string? AltText { get; set; }
        public bool IsMain { get; set; }
        public int DisplayOrder { get; set; }
    }
}
