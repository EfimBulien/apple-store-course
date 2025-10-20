using Microsoft.AspNetCore.Mvc;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Interfaces;
using TechStoreEll.Core.Services;

namespace TechStoreEll.Api.Controllers;

[ApiController]
public class ReviewsController(
    IGenericRepository<Review> repository,
    ILogger<ReviewsController> logger)
    : EntityController<Review>(repository, logger);