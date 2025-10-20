using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Interfaces;
using TechStoreEll.Core.Services;

namespace TechStoreEll.Api.Controllers;

public class PaymentsController(
    IGenericRepository<Payment> repository, 
    ILogger<PaymentsController> logger)
    : EntityController<Payment>(repository, logger);
