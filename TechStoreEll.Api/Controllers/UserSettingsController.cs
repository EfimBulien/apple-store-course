using TechStoreEll.Api.Data;
using TechStoreEll.Api.Models;

namespace TechStoreEll.Api.Controllers;

public class UserSettingsController(AppDbContext context) : EntityController<UserSetting>(context)
{
}
