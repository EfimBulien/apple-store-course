using TechStoreEll.Api.Attributes;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Interfaces;
using TechStoreEll.Core.Services;

namespace TechStoreEll.Api.Controllers;

[AuthorizeRole("Admin")]
public class CategoriesController(
    IGenericRepository<Category> repository, 
    ILogger<CategoriesController> logger) 
    : EntityController<Category>(repository, logger);
