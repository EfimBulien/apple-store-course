using TechStoreEll.Api.Attributes;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Services;
using TechStoreEll.Core.Services.IServices;

namespace TechStoreEll.Api.Controllers;

[AuthorizeRole("Admin")]
public class CategoriesController(
    IGenericRepository<Category> repository, 
    ILogger<CategoriesController> logger) 
    : EntityController<Category>(repository, logger);
