using TechStoreEll.Api.Infrastructure.Data;
using TechStoreEll.Api.Models;

namespace TechStoreEll.Api.Controllers;

public class ProductVariantsController(AppDbContext context, ILogger<EntityController<ProductVariant>> logger) : 
    EntityController<ProductVariant>(context, logger);
