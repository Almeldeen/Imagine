using Application.Common.Models;
using Application.Features.Carts.DTOs;
using Core.Interfaces;
using MediatR;
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
            var cartItems = await _cartRepo.GetAllItemsAsync();

            if (cartItems == null || !cartItems.Any())
                return BaseResponse<List<CartItemDto>>.FailureResponse("No carts found");
            var cartItemDto = new List<CartItemDto>();
            foreach (var item in cartItems) {
                var dto = new CartItemDto
                {
                    Id = item.Id,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice
                };
                cartItemDto.Add(dto);
            }


            return BaseResponse<List<CartItemDto>>.SuccessResponse(cartItemDto, "All cartItems retrieved successfully");
        }
    }
}
