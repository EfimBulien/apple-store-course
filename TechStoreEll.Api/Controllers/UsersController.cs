using TechStoreEll.Api.Attributes;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Interfaces;
using TechStoreEll.Core.Services;

namespace TechStoreEll.Api.Controllers;

[AuthorizeRole("Admin")] // доступ только для администратора
public class UsersController(
    IGenericRepository<User> repository, 
    ILogger<UsersController> logger) 
    : EntityController<User> (repository, logger);
