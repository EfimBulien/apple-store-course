using TechStoreEll.Api.Entities;
using TechStoreEll.Api.Services;

namespace TechStoreEll.Api.Controllers;

public class PaymentsController(
    IGenericRepository<Payment> repository, 
    ILogger<PaymentsController> logger)
    : EntityController<Payment>(repository, logger);
