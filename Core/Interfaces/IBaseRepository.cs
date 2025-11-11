namespace Core.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        IQueryable<T> GetAllQueryable();
        Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    }
}
