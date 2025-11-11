namespace Core.Entities
{
    public class ProductImage : BaseEntity
    {
        public int ProductColorId { get; set; }
        public string ImageUrl { get; set; } = null!;
        public string? AltText { get; set; }
        public bool IsMain { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;

        // Navigation Properties
        public ProductColor ProductColor { get; set; } = null!;
    }
}
