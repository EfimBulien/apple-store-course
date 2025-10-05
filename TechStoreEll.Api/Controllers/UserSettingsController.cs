using TechStoreEll.Api.Entities;
using TechStoreEll.Api.Services;

namespace TechStoreEll.Api.Controllers;

public class UserSettingsController(
    IGenericRepository<UserSetting> repository, 
    ILogger<UserSettingsController> logger) 
    : EntityController<UserSetting> (repository, logger);
