using Core.Entities;
using Core.Interfaces;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class ProductColorRepository : BaseRepository<ProductColor>, IProductColorRepository
    {
        public ProductColorRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
