using TechStoreEll.Api.Models;
using TechStoreEll.Api.Services;

namespace TechStoreEll.Api.Controllers;

public class UserSettingsController(
    IGenericRepository<User> repository, 
    ILogger<UserSettingsController> logger) 
    : EntityController<User> (repository, logger);
