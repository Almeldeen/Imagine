namespace Application.Features.Products.DTOs
{
    /// <summary>
    /// Simple input DTO used when updating an existing product.
    /// </summary>
    public class UpdateProductDto : CreateProductDto
    {
        public int Id { get; set; }
    }
}
