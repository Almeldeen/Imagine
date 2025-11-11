using Core.Entities;
using Core.Interfaces;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class WishlistItemRepository : BaseRepository<WishlistItem>, IWishlistItemRepository
    {
        public WishlistItemRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
