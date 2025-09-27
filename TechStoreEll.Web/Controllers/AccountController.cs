using Microsoft.AspNetCore.Mvc;
using TechStoreEll.Api.Attributes;

namespace TechStoreEll.Web.Controllers;

public class AccountController : Controller
{
    [AuthorizeRole("User", "Admin")]
    public IActionResult Profile()
    {
        return View();
    }

    [AuthorizeRole("Admin")]
    public IActionResult AdminPanel()
    {
        return View();
    }
}