using TechStoreEll.Api.Data;
using TechStoreEll.Api.Models;

namespace TechStoreEll.Api.Controllers;

public class CategorysController(AppDbContext context) : EntityController<Category>(context)
{
}
