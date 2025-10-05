using TechStoreEll.Api.Attributes;
using TechStoreEll.Api.Entities;
using TechStoreEll.Api.Services;

namespace TechStoreEll.Api.Controllers;

[AuthorizeRole("Admin")] // доступ только для администратора
public class RolesController(
    IGenericRepository<Role> repository, 
    ILogger<RolesController> logger) 
    : EntityController<Role> (repository, logger);
