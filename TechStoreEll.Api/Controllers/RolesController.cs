using TechStoreEll.Api.Attributes;
using TechStoreEll.Api.Infrastructure.Data;
using TechStoreEll.Api.Models;

namespace TechStoreEll.Api.Controllers;

[AuthorizeRole("Admin")] // админ
public class RolesController(AppDbContext context, ILogger<EntityController<Role>> logger) : 
    EntityController<Role>(context, logger);
