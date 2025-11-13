using Core.Entities;

namespace Core.Interfaces
{
    public interface ICartItemRepository : IBaseRepository<CartItem>
    {
        public Task<Cart?> GetCartByIdAsync(int cartId);

        public Task<CartItem?> GetCartItemByIdAsync(int id);

        public Task<Cart?> GetCartWithItemsAsync(string userIdOrSessionId);
        public Task UpdateCartItemAsync(CartItem item, CancellationToken cancellationToken);
    }
}
