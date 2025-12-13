namespace Core.Entities
{
    public class CartItem : BaseEntity
    {
        public int CartId { get; set; }
        public int? ProductColorId { get; set; }
        public int? CustomProductId { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Size { get; set; }

        // Navigation Properties
        public Cart Cart { get; set; } = null!;
        public ProductColor? ProductColor { get; set; }
        public CustomProduct? CustomProduct { get; set; }
    }
}
