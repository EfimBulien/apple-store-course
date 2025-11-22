using Microsoft.AspNetCore.Mvc;
using TechStoreEll.Web.Models;

namespace TechStoreEll.Web.Controllers;

public class ErrorController : Controller
{
    [Route("/error/cart")]
    public IActionResult Database([FromQuery] string? message = null)
    {
        var model = new DatabaseErrorViewModel
        {
            Message = message ?? "Не удалось установить соединение с сервисом корзины.",
            DetailedError = ""
        };
        return View("ErrorCart", model);
    }

    [Route("/error")]
    public IActionResult Index() => View();
}