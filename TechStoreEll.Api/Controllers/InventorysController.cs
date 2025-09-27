using TechStoreEll.Api.Infrastructure.Data;
using TechStoreEll.Api.Models;

namespace TechStoreEll.Api.Controllers;

public class InventorysController(AppDbContext context, ILogger<EntityController<Inventory>> logger) : 
    EntityController<Inventory>(context, logger);
