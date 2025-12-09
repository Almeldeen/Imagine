using Core.Entities;
using Core.Interfaces;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class OrderTrackingRepository : BaseRepository<OrderTracking>, IOrderTrackingRepository
    {
        public OrderTrackingRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
