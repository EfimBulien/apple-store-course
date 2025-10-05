using TechStoreEll.Api.Attributes;
using TechStoreEll.Api.Models;
using TechStoreEll.Api.Services;

namespace TechStoreEll.Api.Controllers;

[AuthorizeRole("Admin")]
public class CategoriesController(
    IGenericRepository<Category> repository, 
    ILogger<CategoriesController> logger) 
    : EntityController<Category>(repository, logger);
