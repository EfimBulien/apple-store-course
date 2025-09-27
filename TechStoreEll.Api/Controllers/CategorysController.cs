using TechStoreEll.Api.Infrastructure.Data;
using TechStoreEll.Api.Models;

namespace TechStoreEll.Api.Controllers;

public class CategorysController(AppDbContext context, ILogger<EntityController<Category>> logger) :
    EntityController<Category>(context, logger);
