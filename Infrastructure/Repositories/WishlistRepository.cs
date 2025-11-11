using Core.Entities;
using Core.Interfaces;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class WishlistRepository : BaseRepository<Wishlist>, IWishlistRepository
    {
        public WishlistRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
