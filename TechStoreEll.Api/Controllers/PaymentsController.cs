using TechStoreEll.Api.Data;
using TechStoreEll.Api.Models;

namespace TechStoreEll.Api.Controllers;

public class PaymentsController(AppDbContext context) : EntityController<Payment>(context)
{
}
