using TechStoreEll.Api.Models;
using TechStoreEll.Api.Services;

namespace TechStoreEll.Api.Controllers;

public class ProductImagesController(
    IGenericRepository<ProductImage> repository,
    ILogger<ProductImagesController> logger) 
    : EntityController<ProductImage>(repository, logger);
