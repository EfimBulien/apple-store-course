using TechStoreEll.Api.Data;
using TechStoreEll.Api.Models;

namespace TechStoreEll.Api.Controllers;

public class RolesController(AppDbContext context) : EntityController<Role>(context);