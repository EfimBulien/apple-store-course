using TechStoreEll.Api.Data;
using TechStoreEll.Api.Models;

namespace TechStoreEll.Api.Controllers;

public class AddresssController(AppDbContext context) : EntityController<Address>(context)
{
}
