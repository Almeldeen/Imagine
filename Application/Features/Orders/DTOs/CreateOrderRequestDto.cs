using System.Collections.Generic;

namespace Application.Features.Orders.DTOs
{
    public class CreateOrderRequestDto
    {
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? Notes { get; set; }

        /// <summary>
        /// The cart key used on the frontend (userId or guest session id) to resolve the current cart.
        /// </summary>
        public string CartUserOrSessionId { get; set; } = string.Empty;

        /// <summary>
        /// Grand total as calculated on the client (used for a basic mismatch check).
        /// </summary>
        public decimal GrandTotal { get; set; }
    }
}

