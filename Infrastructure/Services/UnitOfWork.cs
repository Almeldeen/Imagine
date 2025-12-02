using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Services
{
    /// <summary>
    /// EF Core based implementation of IUnitOfWork using ApplicationDbContext.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
        {
            // Use explicit transaction to ensure all operations either commit or rollback together
            await using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await action(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
