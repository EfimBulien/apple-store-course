using TechStoreEll.Api.Attributes;
using TechStoreEll.Api.Entities;
using TechStoreEll.Api.Services;

namespace TechStoreEll.Api.Controllers;

[AuthorizeRole("Admin", "Customer")]
public class CustomersController(
    IGenericRepository<Customer> repository, 
    ILogger<CustomersController> logger) 
    : EntityController<Customer>(repository, logger);
