using TechStoreEll.Api.Attributes;
using TechStoreEll.Api.Infrastructure.Data;
using TechStoreEll.Api.Models;

namespace TechStoreEll.Api.Controllers;

[AuthorizeRole("Admin")] // админ
public class UsersController(AppDbContext context, ILogger<EntityController<User>> logger) : 
    EntityController<User>(context, logger);
