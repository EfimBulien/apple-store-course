using TechStoreEll.Api.Attributes;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Services;
using TechStoreEll.Core.Services.IServices;

namespace TechStoreEll.Api.Controllers;

[AuthorizeRole("Admin")] // доступ только для администратора
public class RolesController(
    IGenericRepository<Role> repository, 
    ILogger<RolesController> logger) 
    : EntityController<Role> (repository, logger);
