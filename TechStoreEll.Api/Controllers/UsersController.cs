using TechStoreEll.Api.Attributes;
using TechStoreEll.Api.Entities;
using TechStoreEll.Api.Services;

namespace TechStoreEll.Api.Controllers;

[AuthorizeRole("Admin")] // доступ только для администратора
public class UsersController(
    IGenericRepository<User> repository, 
    ILogger<UsersController> logger) 
    : EntityController<User> (repository, logger);
