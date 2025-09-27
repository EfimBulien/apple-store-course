using TechStoreEll.Api.Infrastructure.Data;
using TechStoreEll.Api.Models;

namespace TechStoreEll.Api.Controllers;

public class InventoryMovementsController(AppDbContext context, ILogger<EntityController<InventoryMovement>> logger) : 
    EntityController<InventoryMovement>(context, logger);
