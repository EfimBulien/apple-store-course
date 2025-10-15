using Microsoft.AspNetCore.Mvc;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Services;
using TechStoreEll.Core.Services.IServices;

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