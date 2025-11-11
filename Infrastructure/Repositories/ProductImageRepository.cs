using Core.Entities;
using Core.Interfaces;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class ProductImageRepository : BaseRepository<ProductImage>, IProductImageRepository
    {
        public ProductImageRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
