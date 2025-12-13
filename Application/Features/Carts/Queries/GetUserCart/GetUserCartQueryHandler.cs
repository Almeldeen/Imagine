using Application.Common.Models;
using Application.Features.Carts.DTOs;
using Core.Interfaces;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Carts.Queries.GetUserCart
{
    public class GetUserCartQueryHandler : IRequestHandler<GetUserCartQuery, BaseResponse<CartDto>>
    {
        private readonly ICartRepository _cartRepo;

        public GetUserCartQueryHandler(ICartRepository cartRepo)
        {
            _cartRepo = cartRepo;
        }

        public async Task<BaseResponse<CartDto>> Handle(GetUserCartQuery request, CancellationToken cancellationToken)
        {
            var cart = await _cartRepo.GetCartWithItemsAsync(request.UserOrSessionId);

            if (cart == null)
            {
                var empty = new CartDto
                {
                    UserId = null,
                    SessionId = request.UserOrSessionId,
                    ExpiresAt = null,
                    TotalAmount = 0,
                    TotalItems = 0,
                    Items = new System.Collections.Generic.List<CartItemDto>()
                };

                return BaseResponse<CartDto>.SuccessResponse(empty, "Cart is empty");
            }

            var dto = new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                SessionId = cart.SessionId,
                ExpiresAt = cart.ExpiresAt
            };

            foreach (var item in cart.Items)
            {
                string name = "Item";
                string image = string.Empty;
                string color = string.Empty;
                string? size = item.Size;
                decimal basePrice = item.UnitPrice;
                bool isAiPowered = false;

                if (item.ProductColor != null)
                {
                    var product = item.ProductColor.Product;
                    name = product?.Name ?? name;
                    color = item.ProductColor.ColorName;

                    var mainImage = item.ProductColor.Images?.FirstOrDefault(i => i.IsMain);
                    image = mainImage?.ImageUrl ?? product?.MainImageUrl ?? image;

                    if (basePrice <= 0 && product != null)
                    {
                        basePrice = product.BasePrice + item.ProductColor.AdditionalPrice;
                    }
                }
                else if (item.CustomProduct != null)
                {
                    var custom = item.CustomProduct;
                    name = (custom.Product?.Name ?? "Custom product") + " (Custom)";
                    image = custom.AIRenderedPreviewUrl ?? custom.CustomDesignImageUrl ?? image;
                    color = custom.CustomColors?.FirstOrDefault()?.ColorName ?? color;

                    if (basePrice <= 0)
                    {
                        basePrice = custom.EstimatedPrice;
                    }

                    isAiPowered = !string.IsNullOrWhiteSpace(custom.AIRenderedPreviewUrl);
                }

                dto.Items.Add(new CartItemDto
                {
                    Id = item.Id,
                    Name = name,
                    Image = image,
                    Color = color,
                    Size = size,
                    BasePrice = basePrice,
                    Quantity = item.Quantity,
                    IsAiPowered = isAiPowered
                });
            }

            dto.TotalAmount = dto.Items.Sum(i => i.BasePrice * i.Quantity);
            dto.TotalItems = dto.Items.Sum(i => i.Quantity);

            return BaseResponse<CartDto>.SuccessResponse(dto, "Cart retrieved successfully");
        }
    }
}
