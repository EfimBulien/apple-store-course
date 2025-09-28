using Microsoft.AspNetCore.Mvc;
using TechStoreEll.Api.Services;
using TechStoreEll.Api.DTOs;

namespace TechStoreEll.Web.Controllers;

public class AuthController(AuthService authService, JwtService jwtService) : Controller
{
    [HttpGet]
    public IActionResult SignIn() => View();

    [HttpPost]
    public async Task<IActionResult> SignIn(LoginDto dto)
    {
        var user = await authService.Authenticate(dto);
        if (user == null)
        {
            ModelState.AddModelError("", "Неверный логин или пароль");
            return View(dto);
        }

        var token = jwtService.GenerateToken(user);

        
        Response.Cookies.Append("AuthToken", token, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Secure = false,
            Expires = DateTime.UtcNow.AddHours(1)
        });

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var result = await authService.Register(dto);
        if (result) return RedirectToAction("Login");
        ModelState.AddModelError("", "Такой пользователь уже существует");
        return View(dto);

    }

    [HttpPost]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("AuthToken");
        return RedirectToAction("Index", "Home");
    }
}