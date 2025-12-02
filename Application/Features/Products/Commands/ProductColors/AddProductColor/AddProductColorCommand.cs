using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Commands.ProductColors.AddProductColor
{
    public class AddProductColorCommand : IRequest<BaseResponse<int>>
    {
        public int ProductId { get; set; }
        public string ColorName { get; set; } = string.Empty;
        public string? ColorHex { get; set; }
        public int Stock { get; set; }
        public decimal AdditionalPrice { get; set; }
        public bool IsAvailable { get; set; } = true;
    }
}
