using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStoreEll.Api.Data;

namespace TechStoreEll.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public abstract class EntityController<TEntity>(AppDbContext context) : ControllerBase 
    where TEntity : class
{
    private readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();

    /// <summary>
    /// Получить все сущности
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TEntity>>> GetAll()
    {
        var entities = await _dbSet.ToListAsync();
        return Ok(entities);
    }

    /// <summary>
    /// Получить сущность по ID
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TEntity>> Get(int id)
    {
        var entity = await _dbSet.FindAsync(id);
        
        if (entity == null)
        {
            return NotFound();
        }
        
        return Ok(entity);
    }

    /// <summary>
    /// Создать новую сущность
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TEntity>> Post([FromBody] TEntity? entity)
    {
        if (entity == null)
        {
            return BadRequest("Entity cannot be null.");
        }

        _dbSet.Add(entity);
        await context.SaveChangesAsync();

        // Получаем ID созданной сущности (если поддерживается)
        var id = GetEntityId(entity);
        return CreatedAtAction(nameof(Get), new { id }, entity);
    }

    /// <summary>
    /// Обновить существующую сущность
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult> Put(int id, [FromBody] TEntity? entity)
    {
        if (entity == null)
        {
            return BadRequest("Entity cannot be null.");
        }

        if (!EntityIdMatches(id, entity))
        {
            return BadRequest("ID in URL does not match ID in entity body.");
        }

        var existingEntity = await _dbSet.FindAsync(id);
        if (existingEntity == null)
        {
            return NotFound();
        }

        context.Entry(existingEntity).CurrentValues.SetValues(entity);
        await context.SaveChangesAsync();

        return NoContent(); // 204 — успешно обновлено
    }

    /// <summary>
    /// Удалить сущность по ID
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _dbSet.FindAsync(id);
        
        if (entity == null)
        {
            return NotFound();
        }

        _dbSet.Remove(entity);
        await context.SaveChangesAsync();
        
        return NoContent(); // 204 — успешно удалено
    }

    /// <summary>
    /// Вспомогательный метод: получить ID сущности через рефлексию
    /// Ищет свойство Id или IdEntity типа int
    /// </summary>
    private static int GetEntityId(TEntity entity)
    {
        var prop = typeof(TEntity).GetProperty("Id") 
               ?? typeof(TEntity).GetProperty("IdEntity");

        if (prop == null)
        {
            throw new InvalidOperationException($"Entity {
                typeof(TEntity).Name} must have an 'Id' or 'IdEntity' property of type int.");
        }

        var value = prop.GetValue(entity);
        
        switch (value)
        {
            case null:
                return 0;
            // Поддержка как int так и string
            case int intValue:
                return intValue;
        }

        return int.TryParse(value.ToString(), out var parsedInt) ? parsedInt : 0;
    }

    /// <summary>
    /// Проверяет, совпадает ли ID из URL с ID в объекте
    /// </summary>
    private static bool EntityIdMatches(int id, TEntity entity)
    {
        return GetEntityId(entity) == id;
    }
}