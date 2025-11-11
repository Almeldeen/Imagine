using Core.Entities;
using Core.Interfaces;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class OrderRepository : BaseRepository<Order>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
