using Application.Common.Models;
using Application.Features.Carts.DTOs;
using Core.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Carts.Commands.UpdateCartItem
{
    public class UpdateCartItemCommandHandler : IRequestHandler<UpdateCartItemCommand, BaseResponse<CartDto>>
    {
        private readonly ICartItemRepository _cartItemRepo;

        public UpdateCartItemCommandHandler(ICartItemRepository cartItemRepo)
        {
            _cartItemRepo = cartItemRepo;
        }
        public async Task<BaseResponse<CartDto>> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
        {
            // جلب عنصر الكارت
            var cartItem = await _cartItemRepo.GetCartItemByIdAsync(request.CartItemId);
            if (cartItem == null)
                return BaseResponse<CartDto>.FailureResponse("Cart item not found");

            // تحديث الكمية والقيمة الإجمالية
            if (request.NewQuantity.HasValue)
            {
                cartItem.Quantity = request.NewQuantity.Value;
                cartItem.TotalPrice = cartItem.UnitPrice * cartItem.Quantity;
            }

            // حفظ التعديلات على العنصر
            await _cartItemRepo.UpdateAsync(cartItem, cancellationToken);

            // جلب الكارت الكامل بعد التعديل باستخدام CartId
            var cart = await _cartItemRepo.GetCartByIdAsync(cartItem.CartId); // <- دالة جديدة في الـ repository
            if (cart == null)
                return BaseResponse<CartDto>.FailureResponse("Cart not found");

            // مابينج يدوي للكارت إلى DTO
            var cartDto = new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                SessionId = cart.SessionId,
                ExpiresAt = cart.ExpiresAt,
                TotalItems = cart.Items.Count,
                TotalAmount = cart.Items.Sum(i => i.TotalPrice),
                Items = cart.Items.Select(i => new CartItemDto
                {
                    Id = i.Id,
                    ProductColorId = i.ProductColorId,
                    CustomProductId = i.CustomProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice
                }).ToList()
            };

            return BaseResponse<CartDto>.SuccessResponse(cartDto, "Cart item updated successfully");
        }

    }
}
