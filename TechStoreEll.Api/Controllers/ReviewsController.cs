using TechStoreEll.Api.Data;
using TechStoreEll.Api.Models;

namespace TechStoreEll.Api.Controllers;

public class ReviewsController(AppDbContext context) : EntityController<Review>(context)
{
}
