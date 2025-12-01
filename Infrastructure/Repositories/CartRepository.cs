using Application.Features.Carts.DTOs;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;

namespace Infrastructure.Repositories
{
    public class CartRepository : BaseRepository<Cart>, ICartRepository
    {
        #region Fields
      
        #endregion
        #region Ctor

        public CartRepository(ApplicationDbContext context) : base(context)
        {
            
        }
        #endregion
        #region Handele Functions

        public async Task<Cart?> GetCartWithItemsAsync(string userIdOrSessionId)
        {
            return await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.ProductColor)
                        .ThenInclude(pc => pc.Images)
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.ProductColor)
                        .ThenInclude(pc => pc.Product) // آمن ومفيش Colors
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.CustomProduct)
                        .ThenInclude(cp => cp.CustomColors)
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.CustomProduct)
                        .ThenInclude(cp => cp.Product)
                .FirstOrDefaultAsync(c =>
                    c.UserId == userIdOrSessionId ||
                    c.SessionId == userIdOrSessionId);
        }


        public async Task<List<CartItem>> GetAllItemsAsync()
        {
            return await _context.CartItems
                .Include(ci => ci.ProductColor)
                    .ThenInclude(pc => pc.Images)
                .Include(ci => ci.ProductColor)
                    .ThenInclude(pc => pc.Product)
                .Include(ci => ci.CustomProduct)
                    .ThenInclude(cp => cp.CustomColors)
                .Include(ci => ci.CustomProduct)
                    .ThenInclude(cp => cp.Product)
                .ToListAsync();
        }

        public async Task<List<CartItem>> GetAllItemsWithDetailsAsync(CancellationToken cancellationToken)
        {
            return await _context.CartItems
                .Include(ci => ci.ProductColor)
                    .ThenInclude(pc => pc.Product)
                        
                .Include(ci => ci.CustomProduct)
                    .ThenInclude(cp => cp.CustomColors)
                .Include(ci => ci.CustomProduct)
                    .ThenInclude(cp => cp.Product)
                .ToListAsync(cancellationToken);
        }


        public async Task ClearCartAsync(int cartId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cartId);

            if (cart != null)
            {
                _context.CartItems.RemoveRange(cart.Items);
                await _context.SaveChangesAsync();
            }

        }
        public async Task SaveChangeAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        public async Task AddCartItemAsync(CartItem item, CancellationToken cancellationToken)
        {
            await _context.CartItems.AddAsync(item, cancellationToken);
        }

        

        #endregion



    }

}
