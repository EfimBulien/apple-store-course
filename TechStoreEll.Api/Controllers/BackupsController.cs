using TechStoreEll.Api.Infrastructure.Data;
using TechStoreEll.Api.Models;

namespace TechStoreEll.Api.Controllers;

public class BackupsController(AppDbContext context, ILogger<EntityController<Backup>> logger) : 
    EntityController<Backup>(context, logger);
