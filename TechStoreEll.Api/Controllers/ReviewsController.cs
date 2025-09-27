using TechStoreEll.Api.Infrastructure.Data;
using TechStoreEll.Api.Models;

namespace TechStoreEll.Api.Controllers;

public class ReviewsController(AppDbContext context, ILogger<EntityController<Review>> logger) : 
    EntityController<Review>(context, logger);
