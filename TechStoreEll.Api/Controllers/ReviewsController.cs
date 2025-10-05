using Microsoft.AspNetCore.Mvc;
using TechStoreEll.Api.Entities;
using TechStoreEll.Api.Services;

namespace TechStoreEll.Api.Controllers;

[ApiController]
public class ReviewsController(
    IGenericRepository<Review> repository,
    ILogger<ReviewsController> logger)
    : EntityController<Review>(repository, logger)
{
    // Все CRUD-методы уже реализованы в базовом абстрактном классе
    // Можно добавить кастомные методы если нужно, например:
    
    // [HttpGet("something")]
    // public async Task<ActionResult<IEnumerable<Something>>> GetExpensiveSomething()
    // {
    //     // ... реализация
    // }
}