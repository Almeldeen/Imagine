using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Carts.DTOs
{

    public class CartItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Image { get; set; } = "";
        public string Color { get; set; } = "";
        public string? Size { get; set; }
        public decimal BasePrice { get; set; }
        public int Quantity { get; set; }
        public bool IsAiPowered { get; set; }
    }

}
