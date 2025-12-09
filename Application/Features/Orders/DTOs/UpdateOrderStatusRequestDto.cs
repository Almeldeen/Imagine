namespace Application.Features.Orders.DTOs
{
    public class UpdateOrderStatusRequestDto
    {
        public string Status { get; set; } = string.Empty;
        public string? TrackingNumber { get; set; }
    }
}
