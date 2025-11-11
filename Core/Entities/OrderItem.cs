namespace Core.Entities
{
    public class OrderItem : BaseEntity
    {
        public int OrderId { get; set; }
        public int? ProductColorId { get; set; }
        public int? CustomProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? ColorName { get; set; }
        public string? ProductImageUrl { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }

        // Navigation Properties
        public Order Order { get; set; } = null!;
        public ProductColor? ProductColor { get; set; }
        public CustomProduct? CustomProduct { get; set; }
    }
}
