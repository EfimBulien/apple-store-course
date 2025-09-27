using TechStoreEll.Api.Infrastructure.Data;
using TechStoreEll.Api.Models;

namespace TechStoreEll.Api.Controllers;

public class UserSettingsController(AppDbContext context, ILogger<EntityController<UserSetting>> logger) : 
    EntityController<UserSetting>(context, logger);
