using Application.Common.Models;
using Application.Features.Carts.DTOs;
using AutoMapper;
using Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
                return BaseResponse<CartDto>.FailureResponse("Cart not found");

            // مابنج يدوي من Cart → CartDto
            var dto = new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                SessionId = cart.SessionId,
                ExpiresAt = cart.ExpiresAt,
                TotalAmount = cart.Items.Sum(i => i.TotalPrice),
                TotalItems = cart.Items.Sum(i => i.Quantity),
                Items = cart.Items.Select(i => new CartItemDto
                {
                    Id = i.Id,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice
                }).ToList()
            };

            return BaseResponse<CartDto>.SuccessResponse(dto, "Cart retrieved successfully");
        }

    }
}

