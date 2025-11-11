using Core.Entities;
using Core.Interfaces;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class OrderItemRepository : BaseRepository<OrderItem>, IOrderItemRepository
    {
        public OrderItemRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
