namespace Core.Entities
{
    public class CustomProductColor : BaseEntity
    {
        public int CustomProductId { get; set; }
        public string ColorName { get; set; } = null!;
        public string? ColorHex { get; set; }
        public string? ImageUrl { get; set; }

        // Navigation Properties
        public CustomProduct CustomProduct { get; set; } = null!;
    }
}
