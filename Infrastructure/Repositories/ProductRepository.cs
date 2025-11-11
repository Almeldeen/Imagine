using Core.Entities;
using Core.Interfaces;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class ProductRepository : BaseRepository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
