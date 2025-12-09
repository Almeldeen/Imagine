using Core.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IOrderRepository : IBaseRepository<Order>
    {
        Task<Order?> GetWithDetailsByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}
