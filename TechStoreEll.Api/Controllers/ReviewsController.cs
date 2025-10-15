using Microsoft.AspNetCore.Mvc;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Services;
using TechStoreEll.Core.Services.IServices;

namespace TechStoreEll.Api.Controllers;

[ApiController]
public class ReviewsController(
    IGenericRepository<Review> repository,
    ILogger<ReviewsController> logger)
    : EntityController<Review>(repository, logger);