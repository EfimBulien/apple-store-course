using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Services;
using TechStoreEll.Core.Services.IServices;

namespace TechStoreEll.Api.Controllers;

public class UserSettingsController(
    IGenericRepository<UserSetting> repository, 
    ILogger<UserSettingsController> logger) 
    : EntityController<UserSetting> (repository, logger);
