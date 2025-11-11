using Core.Entities;
using Core.Interfaces;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class CustomProductRepository : BaseRepository<CustomProduct>, ICustomProductRepository
    {
        public CustomProductRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
