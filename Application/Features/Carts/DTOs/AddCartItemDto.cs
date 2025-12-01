using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Carts.DTOs
{
    public class AddCartItemDto
    {
        public int CartId { get; set; }
        public int? ProductColorId { get; set; }
        public int? CustomProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
