using Microsoft.EntityFrameworkCore;
using TechStoreEll.Api.Entities;
using TechStoreEll.Api.Infrastructure.Data;

namespace TechStoreEll.Api.Services;

public class GenericRepository<TEntity>(
    AppDbContext context, 
    ILogger<GenericRepository<TEntity>> logger) 
    : IGenericRepository<TEntity>
    where TEntity : class, IEntity
{
    private readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();

    public async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        logger.LogInformation("Получение всех сущностей {EntityType}", typeof(TEntity).Name);
        return await _dbSet.ToListAsync();
    }

    public async Task<TEntity?> GetByIdAsync(int id)
    {
        logger.LogInformation("Поиск сущности {EntityType} по ID {Id}", typeof(TEntity).Name, id);
        return await _dbSet.FindAsync(id);
    }

    public async Task<TEntity> CreateAsync(TEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        _dbSet.Add(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(int id, TEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        var existing = await _dbSet.FindAsync(id);
        if (existing == null) throw new KeyNotFoundException($"Сущность {typeof(TEntity).Name} с ID {id} не найдена");

        context.Entry(existing).CurrentValues.SetValues(entity);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity == null) throw new KeyNotFoundException($"Сущность {typeof(TEntity).Name} с ID {id} не найдена");

        _dbSet.Remove(entity);
        await context.SaveChangesAsync();
    }
}