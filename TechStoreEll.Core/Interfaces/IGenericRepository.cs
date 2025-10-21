using TechStoreEll.Core.Entities;

namespace TechStoreEll.Core.Interfaces;

public interface IGenericRepository<TEntity> where TEntity : class, IEntity
{
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<TEntity?> GetByIdAsync(int id);
    Task<TEntity> CreateAsync(TEntity entity);
    Task UpdateAsync(int id, TEntity entity);
    Task DeleteAsync(int id);
}
