// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using TechStoreEll.Api.Infrastructure.Data;
//
// namespace TechStoreEll.Api.Controllers;
//
// [Route("api/[controller]")]
// [ApiController]
// public abstract class EntityController<TEntity>(AppDbContext context, ILogger<EntityController<TEntity>> logger) 
//     : ControllerBase 
//     where TEntity : class
// {
//     private readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();
//
//     /// <summary>
//     /// Получить все сущности
//     /// </summary>
//     [HttpGet]
//     public async Task<ActionResult<IEnumerable<TEntity>>> GetAll()
//     {
//         logger.LogInformation("Начало выполнения GetAll для {EntityType}", typeof(TEntity).Name);
//         try
//         {
//             var entities = await _dbSet.ToListAsync();
//             logger.LogInformation("Успешно получено {EntityCount} сущностей типа {EntityType}", entities.Count, typeof(TEntity).Name);
//             return Ok(entities);
//         }
//         catch (Exception ex)
//         {
//             logger.LogError(ex, "Ошибка при получении сущностей типа {EntityType}", typeof(TEntity).Name);
//             throw;
//         }
//     }
//
//     /// <summary>
//     /// Получить сущность по ID
//     /// </summary>
//     [HttpGet("{id:int}")]
//     public async Task<ActionResult<TEntity>> Get(int id)
//     {
//         logger.LogInformation("Начало выполнения Get для {EntityType} с ID {Id}", typeof(TEntity).Name, id);
//         try
//         {
//             var entity = await _dbSet.FindAsync(id);
//             
//             if (entity == null)
//             {
//                 logger.LogWarning("Сущность типа {EntityType} с ID {Id} не найдена", typeof(TEntity).Name, id);
//                 return NotFound("Сущность не найдена");
//             }
//             
//             logger.LogInformation("Успешно получена сущность {EntityType} с ID {Id}", typeof(TEntity).Name, id);
//             return Ok(entity);
//         }
//         catch (Exception ex)
//         {
//             logger.LogError(ex, "Ошибка при получении сущности {EntityType} с ID {Id}", typeof(TEntity).Name, id);
//             throw;
//         }
//     }
//
//     /// <summary>
//     /// Создать новую сущность
//     /// </summary>
//     [HttpPost]
//     public async Task<ActionResult<TEntity>> Post([FromBody] TEntity? entity)
//     {
//         logger.LogInformation("Начало выполнения Post для {EntityType}", typeof(TEntity).Name);
//         try
//         {
//             if (entity == null)
//             {
//                 logger.LogWarning("Получена пустая сущность в Post для {EntityType}", typeof(TEntity).Name);
//                 return BadRequest("Сущность не может быть пустой");
//             }
//
//             _dbSet.Add(entity);
//             await context.SaveChangesAsync();
//
//             var id = GetEntityId(entity);
//             logger.LogInformation("Успешно создана сущность {EntityType} с ID {Id}", typeof(TEntity).Name, id);
//             return CreatedAtAction(nameof(Get), new { id }, entity);
//         }
//         catch (Exception ex)
//         {
//             logger.LogError(ex, "Ошибка при создании сущности {EntityType}", typeof(TEntity).Name);
//             throw;
//         }
//     }
//
//     /// <summary>
//     /// Обновить существующую сущность
//     /// </summary>
//     [HttpPut("{id:int}")]
//     public async Task<ActionResult> Put(int id, [FromBody] TEntity? entity)
//     {
//         logger.LogInformation("Начало выполнения Put для {EntityType} с ID {Id}", typeof(TEntity).Name, id);
//         try
//         {
//             if (entity == null)
//             {
//                 logger.LogWarning("Получена пустая сущность в Put для {EntityType} с ID {Id}", typeof(TEntity).Name, id);
//                 return BadRequest("Сущность не может быть пустой");
//             }
//
//             if (!EntityIdMatches(id, entity))
//             {
//                 logger.LogWarning("Несоответствие ID в Put для {EntityType}. ID в URL: {UrlId}, ID в сущности: {EntityId}", 
//                     typeof(TEntity).Name, id, GetEntityId(entity));
//                 return BadRequest("ID в URL не совпадает с ID в теле сущности");
//             }
//
//             var existingEntity = await _dbSet.FindAsync(id);
//             if (existingEntity == null)
//             {
//                 logger.LogWarning("Сущность типа {EntityType} с ID {Id} не найдена для обновления", typeof(TEntity).Name, id);
//                 return NotFound("Сущность не найдена");
//             }
//
//             context.Entry(existingEntity).CurrentValues.SetValues(entity);
//             await context.SaveChangesAsync();
//
//             logger.LogInformation("Успешно обновлена сущность {EntityType} с ID {Id}", typeof(TEntity).Name, id);
//             return NoContent();
//         }
//         catch (Exception ex)
//         {
//             logger.LogError(ex, "Ошибка при обновлении сущности {EntityType} с ID {Id}", typeof(TEntity).Name, id);
//             throw;
//         }
//     }
//
//     /// <summary>
//     /// Удалить сущность по ID
//     /// </summary>
//     [HttpDelete("{id:int}")]
//     public async Task<IActionResult> Delete(int id)
//     {
//         logger.LogInformation("Начало выполнения Delete для {EntityType} с ID {Id}", typeof(TEntity).Name, id);
//         try
//         {
//             var entity = await _dbSet.FindAsync(id);
//             
//             if (entity == null)
//             {
//                 logger.LogWarning("Сущность типа {EntityType} с ID {Id} не найдена для удаления", typeof(TEntity).Name, id);
//                 return NotFound("Сущность не найдена");
//             }
//
//             _dbSet.Remove(entity);
//             await context.SaveChangesAsync();
//             
//             logger.LogInformation("Успешно удалена сущность {EntityType} с ID {Id}", typeof(TEntity).Name, id);
//             return NoContent();
//         }
//         catch (Exception ex)
//         {
//             logger.LogError(ex, "Ошибка при удалении сущности {EntityType} с ID {Id}", typeof(TEntity).Name, id);
//             throw;
//         }
//     }
//
//     /// <summary>
//     /// Вспомогательный метод: получить ID сущности через рефлексию
//     /// Ищет свойство Id или IdEntity типа int
//     /// </summary>
//     private static int GetEntityId(TEntity entity)
//     {
//         var prop = typeof(TEntity).GetProperty("Id") 
//                ?? typeof(TEntity).GetProperty("IdEntity");
//
//         if (prop == null)
//         {
//             throw new InvalidOperationException($"Сущность {typeof(TEntity).Name} должна иметь свойство 'Id' или 'IdEntity' типа int.");
//         }
//
//         var value = prop.GetValue(entity);
//
//         return value switch
//         {
//             null => 0,
//             int intValue => intValue,
//             _ => int.TryParse(value.ToString(), out var parsedInt) ? parsedInt : 0
//         };
//     }
//
//     /// <summary>
//     /// Проверяет, совпадает ли ID из URL с ID в объекте
//     /// </summary>
//     private static bool EntityIdMatches(int id, TEntity entity)
//     {
//         return GetEntityId(entity) == id;
//     }
// }



using Microsoft.AspNetCore.Mvc;
using TechStoreEll.Api.Entities;
using TechStoreEll.Api.Services;

namespace TechStoreEll.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public abstract class EntityController<TEntity>(
    IGenericRepository<TEntity> repository,
    ILogger<EntityController<TEntity>> logger)
    : ControllerBase
    where TEntity : class, IEntity
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TEntity>>> GetAll()
    {
        logger.LogInformation("Начало выполнения GetAll для {EntityType}", typeof(TEntity).Name);
        try
        {
            var entities = await repository.GetAllAsync();
            logger.LogInformation("Успешно получено {Count} сущностей типа {EntityType}", entities.Count(), typeof(TEntity).Name);
            return Ok(entities);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при получении всех сущностей типа {EntityType}", typeof(TEntity).Name);
            return StatusCode(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера");
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TEntity>> Get(int id)
    {
        logger.LogInformation("Начало выполнения Get для {EntityType} с ID {Id}", typeof(TEntity).Name, id);
        try
        {
            var entity = await repository.GetByIdAsync(id);
            if (entity == null)
            {
                logger.LogWarning("Сущность типа {EntityType} с ID {Id} не найдена", typeof(TEntity).Name, id);
                return NotFound("Сущность не найдена");
            }

            logger.LogInformation("Успешно получена сущность {EntityType} с ID {Id}", typeof(TEntity).Name, id);
            return Ok(entity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при получении сущности {EntityType} с ID {Id}", typeof(TEntity).Name, id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера");
        }
    }

    [HttpPost]
    public async Task<ActionResult<TEntity>> Post([FromBody] TEntity? entity)
    {
        logger.LogInformation("Начало выполнения Post для {EntityType}", typeof(TEntity).Name);
        try
        {
            if (entity == null)
            {
                logger.LogWarning("Получена пустая сущность в Post для {EntityType}", typeof(TEntity).Name);
                return BadRequest("Сущность не может быть пустой");
            }

            var createdEntity = await repository.CreateAsync(entity);
            logger.LogInformation("Успешно создана сущность {EntityType} с ID {Id}", typeof(TEntity).Name, createdEntity.Id);
            return CreatedAtAction(nameof(Get), new { id = createdEntity.Id }, createdEntity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при создании сущности {EntityType}", typeof(TEntity).Name);
            return StatusCode(StatusCodes.Status500InternalServerError, "Не удалось создать сущность");
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, [FromBody] TEntity? entity)
    {
        logger.LogInformation("Начало выполнения Put для {EntityType} с ID {Id}", typeof(TEntity).Name, id);
        try
        {
            if (entity == null)
            {
                logger.LogWarning("Получена пустая сущность в Put для {EntityType} с ID {Id}", typeof(TEntity).Name, id);
                return BadRequest("Сущность не может быть пустой");
            }

            if (entity.Id != id)
            {
                logger.LogWarning("Несоответствие ID: URL={UrlId}, Entity.Id={EntityId}", id, entity.Id);
                return BadRequest("ID в URL не совпадает с ID в теле запроса");
            }

            await repository.UpdateAsync(id, entity);
            logger.LogInformation("Успешно обновлена сущность {EntityType} с ID {Id}", typeof(TEntity).Name, id);
            return NoContent();
        }
        catch (KeyNotFoundException knfEx)
        {
            logger.LogWarning(knfEx, "Сущность {EntityType} с ID {Id} не найдена для обновления", typeof(TEntity).Name, id);
            return NotFound("Сущность не найдена");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при обновлении сущности {EntityType} с ID {Id}", typeof(TEntity).Name, id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Не удалось обновить сущность");
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        logger.LogInformation("Начало выполнения Delete для {EntityType} с ID {Id}", typeof(TEntity).Name, id);
        try
        {
            await repository.DeleteAsync(id);
            logger.LogInformation("Успешно удалена сущность {EntityType} с ID {Id}", typeof(TEntity).Name, id);
            return NoContent();
        }
        catch (KeyNotFoundException knfEx)
        {
            logger.LogWarning(knfEx, "Сущность {EntityType} с ID {Id} не найдена для удаления", typeof(TEntity).Name, id);
            return NotFound("Сущность не найдена");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при удалении сущности {EntityType} с ID {Id}", typeof(TEntity).Name, id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Не удалось удалить сущность");
        }
    }
}