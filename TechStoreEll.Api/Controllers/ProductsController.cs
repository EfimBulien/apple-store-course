using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStoreEll.Api.Data;

namespace TechStoreEll.Api.Controllers;

// [ApiController]
// [Route("api/[controller]")]
// public class ProductsController : ControllerBase
// {
//     [HttpGet]
//     public IActionResult Get()
//     {
//         var products = new[]
//         {
//             new { Id = 1, Name = "Ноутбук", Price = 1000 },
//             new { Id = 2, Name = "Смартфон", Price = 600 },
//         };
//         Console.WriteLine("OK");
//         return Ok(products);
//     }
// }

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController(AppDbContext context) : ControllerBase
{
    [HttpGet("search")]
    public async Task<IActionResult> SearchProducts([FromQuery] string name)
    {
        if (string.IsNullOrEmpty(name))
            return BadRequest("Product name is required");

        var products = await context.Products
            .Where(p => p.Name.Contains(name))
            .ToListAsync();

        return Ok(products);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllProducts()
    {
        var products = await context.Products.ToListAsync();
        return Ok(products);
    }
}