using Core.Entities;
using Core.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class CartItemRepository : BaseRepository<CartItem>, ICartItemRepository
    {
        public CartItemRepository(ApplicationDbContext context) : base(context)
        {
        }
        public async Task<Cart?> GetCartWithItemsAsync(string userIdOrSessionId)
        {
            return await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userIdOrSessionId || c.SessionId == userIdOrSessionId);


        }
        public async Task<CartItem?> GetCartItemByIdAsync(int id)
        {
            return await _context.CartItems.FirstOrDefaultAsync(c => c.Id == id);
        }

        // تحديث عنصر الكارت
        public async Task UpdateCartItemAsync(CartItem item, CancellationToken cancellationToken)
        {
            _context.CartItems.Update(item);
            await _context.SaveChangesAsync(cancellationToken);
        }
        public async Task<Cart?> GetCartByIdAsync(int cartId)
        {
            return await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cartId);
        }

    }
}
