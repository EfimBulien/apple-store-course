using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStoreEll.Api.Infrastructure.Data;

namespace TechStoreEll.Web.Controllers
{
    public class HomeController(AppDbContext context) : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public async Task<ViewResult> Privacy()
        {
            var roles = await context.Roles.ToListAsync();
            return View(roles);
         
        }
    }
}