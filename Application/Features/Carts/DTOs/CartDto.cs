namespace Application.Features.Carts.DTOs
{
    public class CartDto
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? SessionId { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalItems { get; set; }

        public List<CartItemDto> Items { get; set; } = new();
    }

    
}
