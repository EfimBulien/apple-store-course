using TechStoreEll.Api.Infrastructure.Data;
using TechStoreEll.Api.Models;

namespace TechStoreEll.Api.Controllers;

public class AddresssController(AppDbContext context, ILogger<EntityController<Address>> logger) :
    EntityController<Address>(context, logger);
