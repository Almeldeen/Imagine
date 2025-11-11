using Core.Entities;
using Core.Interfaces;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class CartRepository : BaseRepository<Cart>, ICartRepository
    {
        public CartRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
