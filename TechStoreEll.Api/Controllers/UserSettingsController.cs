using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Interfaces;
using TechStoreEll.Core.Services;

namespace TechStoreEll.Api.Controllers;

public class UserSettingsController(
    IGenericRepository<UserSetting> repository, 
    ILogger<UserSettingsController> logger) 
    : EntityController<UserSetting> (repository, logger);
