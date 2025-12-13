using Application.Common.Models;
using Application.Features.Carts.DTOs;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Carts.Commands.AddToCart
{
    public class AddToCartHandler
        : IRequestHandler<AddToCartCommand, BaseResponse<bool>>
    {
        private readonly ICartRepository _cartRepo;
        private readonly IProductColorRepository _productColorRepo;

        public AddToCartHandler(ICartRepository cartRepo, IProductColorRepository productColorRepo)
        {
            _cartRepo = cartRepo;
            _productColorRepo = productColorRepo;
        }

        public async Task<BaseResponse<bool>> Handle(AddToCartCommand request, CancellationToken cancellationToken)
        {
            var cart = await _cartRepo.GetCartWithItemsAsync(request.UserOrSessionId);

            if (cart == null)
            {
                cart = new Cart
                {
                    SessionId = request.UserOrSessionId,
                    CreatedAt = DateTime.UtcNow
                };
                await _cartRepo.AddAsync(cart, cancellationToken);
                await _cartRepo.SaveChangeAsync(cancellationToken);
            }

            var existed = cart.Items.FirstOrDefault(i =>
                i.ProductColorId == request.ProductColorId &&
                i.Size == request.Size);

            if (existed != null)
            {
                existed.Quantity += request.Quantity;
                existed.TotalPrice = existed.UnitPrice * existed.Quantity;
            }
            else
            {
                var productColor = await _productColorRepo
                    .GetAllQueryable()
                    .Include(pc => pc.Product)
                    .FirstOrDefaultAsync(pc => pc.Id == request.ProductColorId, cancellationToken);

                if (productColor == null || productColor.Product == null)
                {
                    return BaseResponse<bool>.FailureResponse("Product color not found");
                }

                var unitPrice = productColor.Product.BasePrice + productColor.AdditionalPrice;

                var newItem = new CartItem
                {
                    ProductColorId = request.ProductColorId,
                    Quantity = request.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = unitPrice * request.Quantity,
                    CartId = cart.Id,
                    Size = request.Size
                };

                await _cartRepo.AddCartItemAsync(newItem, cancellationToken);
            }

            await _cartRepo.SaveChangeAsync(cancellationToken);

            return BaseResponse<bool>.SuccessResponse(true, "Added to cart");
        }
    }

}
