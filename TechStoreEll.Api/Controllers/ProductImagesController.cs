using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Interfaces;
using TechStoreEll.Core.Services;

namespace TechStoreEll.Api.Controllers;

public class ProductImagesController(
    IGenericRepository<ProductImage> repository,
    ILogger<ProductImagesController> logger) 
    : EntityController<ProductImage>(repository, logger);
