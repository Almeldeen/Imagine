using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Carts.DTOs
{
    public class AddCartDto
    {
        public string? UserId { get; set; }
        public string? SessionId { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public List<AddCartItemDto> Items { get; set; } = new();
    }
}
