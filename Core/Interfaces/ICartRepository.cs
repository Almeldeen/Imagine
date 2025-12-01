using Core.Entities;

namespace Core.Interfaces
{
    public interface ICartRepository : IBaseRepository<Cart>
    {
        public Task<Cart?> GetCartWithItemsAsync(string userIdOrSessionId);
        public Task<List<CartItem>> GetAllItemsAsync();
        Task<List<CartItem>> GetAllItemsWithDetailsAsync(CancellationToken cancellationToken);

        Task ClearCartAsync(int cartId);
        Task SaveChangeAsync(CancellationToken cancellationToken = default);
        public Task AddCartItemAsync(CartItem item, CancellationToken cancellationToken);
      
        

    }
}
