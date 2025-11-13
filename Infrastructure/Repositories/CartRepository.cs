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
                .FirstOrDefaultAsync(c => c.UserId == userIdOrSessionId || c.SessionId == userIdOrSessionId);


        }
        public async Task<List<CartItem>> GetAllItemsAsync()
        {
            return await _context.CartItems.ToListAsync(); 
                
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
