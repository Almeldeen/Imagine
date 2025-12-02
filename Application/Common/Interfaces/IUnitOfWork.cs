using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Common.Interfaces
{
    /// <summary>
    /// Simple unit of work abstraction used to execute multiple repository operations
    /// within a single database transaction.
    /// </summary>
    public interface IUnitOfWork
    {
        Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default);
    }
}
