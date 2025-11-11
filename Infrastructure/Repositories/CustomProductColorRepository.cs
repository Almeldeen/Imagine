using Core.Entities;
using Core.Interfaces;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class CustomProductColorRepository : BaseRepository<CustomProductColor>, ICustomProductColorRepository
    {
        public CustomProductColorRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
