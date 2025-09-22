using TechStoreEll.Api.Data;
using TechStoreEll.Api.Models;

namespace TechStoreEll.Api.Controllers;

public class UsersController(AppDbContext context) : EntityController<User>(context);