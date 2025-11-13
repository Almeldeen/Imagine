using Application.Common.Models;
using Application.Features.Carts.DTOs;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;

namespace Application.Features.Carts.Commands.AddToCart
{
    public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, BaseResponse<CartDto>>
    {
        private readonly ICartRepository _cartRepo;
      

        public AddToCartCommandHandler(ICartRepository cartRepo)
        {
            _cartRepo = cartRepo;
           
        }
        public async Task<BaseResponse<CartDto>> Handle(AddToCartCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Cart;
            string key = dto.UserId ?? dto.SessionId ?? "";

            // ✅ نحاول نجيب الكارت حسب الـ UserId أو SessionId
            var cart = await _cartRepo.GetCartWithItemsAsync(key);

            if (cart == null)
            {
                // 🟢 لو مفيش كارت نعمل كارت جديد
                cart = new Cart
                {
                    UserId = dto.UserId,
                    SessionId = dto.SessionId,
                    ExpiresAt = dto.ExpiresAt ?? DateTime.UtcNow.AddDays(7)
                };

                await _cartRepo.AddAsync(cart, cancellationToken);
                await _cartRepo.SaveChangeAsync(cancellationToken); // 🔥 مهم لحفظ الكارت قبل إضافة العناصر
                cart = await _cartRepo.GetCartWithItemsAsync(key); // نرجع الكارت بعد الحفظ
            }

            // 🟢 نضيف أو نحدث العناصر
            foreach (var itemDto in dto.Items)
            {
                var existingItem = cart.Items.FirstOrDefault(i =>
                    i.ProductColorId == itemDto.ProductColorId &&
                    i.CustomProductId == itemDto.CustomProductId
                );

                if (existingItem != null)
                {
                    // 🔄 تحديث الكمية والسعر
                    existingItem.Quantity += itemDto.Quantity;
                    existingItem.TotalPrice = existingItem.UnitPrice * existingItem.Quantity;
                }
                else
                {
                    var cartItem = new CartItem
                    {
                        CartId = cart.Id,
                        ProductColorId = itemDto.ProductColorId,
                        CustomProductId = itemDto.CustomProductId,
                        Quantity = itemDto.Quantity,
                        UnitPrice = itemDto.UnitPrice,
                        TotalPrice = itemDto.UnitPrice * itemDto.Quantity
                    };

                    await _cartRepo.AddCartItemAsync(cartItem, cancellationToken);
                }
            }

            // ✅ نحفظ كل التغييرات
            await _cartRepo.SaveChangeAsync(cancellationToken);

            // 🧩 مابينج يدوي لـ Cart → CartDto
            cart = await _cartRepo.GetCartWithItemsAsync(key); // نجيب الكارت بعد التحديث
            var cartDto = new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                SessionId = cart.SessionId,
                ExpiresAt = cart.ExpiresAt,
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

            cartDto.TotalItems = cartDto.Items.Count;
            cartDto.TotalAmount = cartDto.Items.Sum(i => i.TotalPrice);

            return BaseResponse<CartDto>.SuccessResponse(cartDto, "Items added to cart successfully");
        }



    }
}
