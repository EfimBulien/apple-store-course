using Microsoft.AspNetCore.Mvc;
using TechStoreEll.Core.DTOs;
using TechStoreEll.Core.Services;

namespace TechStoreEll.Web.Controllers;

public class AuthController(AuthService authService, JwtService jwtService) : Controller
{
    private static readonly Dictionary<string, (string Email, DateTime Expires)> ResetTokens = new();
    
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
    public IActionResult SignUp() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SignUp([FromForm] RegisterDto dto)
    {
        if (!ModelState.IsValid)
        {
            Console.WriteLine("Неверные данные модели");
            return View(dto);
        }
    
        var result = await authService.Register(dto);
        if (result) 
        {
            TempData["SuccessMessage"] = "Регистрация прошла успешно! Теперь вы можете войти в систему.";
            return RedirectToAction("Index", "Home");
        }
    
        ModelState.AddModelError("", "Такой пользователь уже существует");
        return View(dto);
    }

    [HttpPost]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("AuthToken");
        return RedirectToAction("Index", "Home");
    }
    
    [HttpGet]
    public IActionResult ForgotPassword() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var user = await authService.GetUserByEmailAsync(dto.Email);
        if (user == null)
        {
            return RedirectToAction("ForgotPasswordConfirmation");
        }

        var token = AuthService.GenerateResetToken();
        var expiration = DateTime.UtcNow.AddHours(1);
        
        ResetTokens[token] = (dto.Email, expiration);

        CleanExpiredTokens();
        
        ViewBag.Email = dto.Email;
        ViewBag.ResetLink = Url.Action("ResetPassword", "Auth", new
        {
            token
        }, protocol: HttpContext.Request.Scheme);
        
        ViewBag.PublicKey = "iy6f9MZAEZbnI3x2y";
        ViewBag.ServiceID = "service_TechStoreEll";
        ViewBag.TemplateID = "template_a9ice16";

        return View("SendResetEmail");
    }

    [HttpGet]
    public IActionResult ForgotPasswordConfirmation() => View();

    [HttpGet]
    public IActionResult SendResetEmail() => View();

    [HttpGet]
    public IActionResult ResetPassword(string token)
    {
        if (string.IsNullOrEmpty(token) || !IsValidToken(token))
        {
            ViewBag.ErrorMessage = "Недействительная или просроченная ссылка для сброса пароля";
            return View("Error");
        }

        var model = new ResetPasswordDto
        {
            Token = token
        };
        
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        if (!IsValidToken(dto.Token))
        {
            ModelState.AddModelError("", "Недействительная или просроченная ссылка для сброса пароля");
            return View(dto);
        }

        var email = ResetTokens[dto.Token].Email;
    
        var isSameAsCurrent = await authService.IsPasswordSameAsCurrentAsync(email, dto.Password);
        if (isSameAsCurrent)
        {
            ModelState.AddModelError("Password", "Новый пароль не должен совпадать с текущим");
            return View(dto);
        }

        var result = await authService.UpdateUserPasswordAsync(email, dto.Password);
    
        if (!result)
        {
            ModelState.AddModelError("", "Ошибка при сбросе пароля");
            return View(dto);
        }
    
        ResetTokens.Remove(dto.Token);
        return RedirectToAction("ResetPasswordConfirmation");
    }

    [HttpGet]
    public IActionResult ResetPasswordConfirmation() => View();

    private static bool IsValidToken(string token)
    {
        return ResetTokens.ContainsKey(token) && ResetTokens[token].Expires > DateTime.UtcNow;
    }

    private static void CleanExpiredTokens()
    {
        var expiredTokens = ResetTokens
            .Where(kvp => kvp.Value.Expires <= DateTime.UtcNow)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var token in expiredTokens) 
            ResetTokens.Remove(token);
    }
}