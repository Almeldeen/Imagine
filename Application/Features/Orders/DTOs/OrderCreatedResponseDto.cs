namespace Application.Features.Orders.DTOs
{
    public class OrderCreatedResponseDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }
}
