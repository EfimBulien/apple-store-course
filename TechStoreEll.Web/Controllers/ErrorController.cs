using Microsoft.AspNetCore.Mvc;
using TechStoreEll.Web.Models;

namespace TechStoreEll.Web.Controllers;

public class ErrorController : Controller
{
    [Route("/error/database")]
    public IActionResult Database([FromQuery] string message = null)
    {
        var model = new DatabaseErrorViewModel
        {
            Message = message ?? "Не удалось установить соединение с базой данных.",
            DetailedError = ""
        };
        return View("ErrorDatabase", model);
    }

    [Route("/error")]
    public IActionResult Index() => View();
}