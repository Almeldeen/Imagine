using Application.Common.Models;
using Application.Features.Carts.DTOs;
using Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Carts.Queries.GetUserCart
{
    public class GetAllCartsQueryHandler : IRequestHandler<GetAllCartsQuery, BaseResponse<List<CartItemDto>>>
    {
        private readonly ICartRepository _cartRepo;

        public GetAllCartsQueryHandler(ICartRepository cartRepo)
        {
            _cartRepo = cartRepo;
        }
        public async Task<BaseResponse<List<CartItemDto>>> Handle(GetAllCartsQuery request, CancellationToken cancellationToken)
        {
            var items = await _cartRepo.GetAllItemsWithDetailsAsync(cancellationToken);

            var dtoList = items.Select(ci => new CartItemDto
            {
                Id = ci.Id,
                Quantity = ci.Quantity,
                BasePrice = ci.UnitPrice,

                Name = ci.ProductColor != null
                    ? ci.ProductColor.Product.Name
                    : ci.CustomProduct != null
                        ? ci.CustomProduct.Product?.Name ?? "Custom Product"
                        : "Product",

                Image = ci.ProductColor != null
                    ? (ci.ProductColor.Images.FirstOrDefault(i => i.IsMain)?.ImageUrl
                        ?? ci.ProductColor.Images.FirstOrDefault()?.ImageUrl
                        ?? ci.ProductColor.Product.MainImageUrl
                        ?? "/assets/images/default.png")
                    : (ci.CustomProduct?.AIRenderedPreviewUrl
                        ?? ci.CustomProduct?.CustomDesignImageUrl
                        ?? "/assets/images/default.png"),

                Color = ci.ProductColor?.ColorName
                    ?? ci.CustomProduct?.CustomColors.FirstOrDefault()?.ColorName
                    ?? "",

                Size = null,
                IsAiPowered = ci.CustomProductId != null
            }).ToList();

            return BaseResponse<List<CartItemDto>>.SuccessResponse(dtoList, "All cartItems retrieved successfully");
        }


    }
}
