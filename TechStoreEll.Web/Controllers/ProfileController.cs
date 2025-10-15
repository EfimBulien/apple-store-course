using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechStoreEll.Core.DTOs;
using TechStoreEll.Core.Models;
using TechStoreEll.Core.Services;
using TechStoreEll.Web.Helpers;

namespace TechStoreEll.Web.Controllers;

public class ProfileController(UserService userService) : Controller
{
    [AuthorizeRole("Customer", "Admin")]
    public async Task<IActionResult> Index()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var user = await userService.GetUserWithSettingsAsync(userId);
        
        if (user == null)
        {
            return NotFound();
        }

        var userDto = new UpdateUserDto
        {
            Email = user.Email,
            Phone = user.Phone ?? "",
            FirstName = user.FirstName,
            LastName = user.LastName,
            MiddleName = user.MiddleName
        };

        var settingsDto = new UpdateUserSettingsDto
        {
            Theme = user.UserSetting?.Theme,
            ItemsPerPage = user.UserSetting?.ItemsPerPage,
            DateFormat = user.UserSetting?.DateFormat,
            NumberFormat = user.UserSetting?.NumberFormat,
            SavedFilters = user.UserSetting?.SavedFilters,
            Hotkeys = user.UserSetting?.Hotkeys
        };

        var model = new ProfileViewModel
        {
            User = userDto,
            Settings = settingsDto
        };

        return View(model);
    }
    
    [HttpPost]
    [AuthorizeRole("Customer", "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
    {
        
        if (!ModelState.IsValid)
        {
            Console.WriteLine("Неверная модель данных");
            return View("Index", model);
        }
        
        if (!ModelState.IsValid)
        {
            Console.WriteLine("Неверная модель данных");
            foreach (var key in ModelState.Keys)
            {
                var state = ModelState[key];
                if (state!.Errors.Count <= 0) continue;
                foreach (var error in state.Errors)
                {
                    Console.WriteLine($"Ошибка в поле '{key}': {error.ErrorMessage}");
                }
            }
            return View("Index", model);
        }

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        if (userId <= 0)
            return BadRequest();

        var result = await userService.UpdateUserAsync(userId, model.User);

        TempData["SuccessMessage"] = result ? "Данные успешно обновлены!" : "Ошибка при обновлении данных!";

        return RedirectToAction("Index");
    }

    [HttpPost]
    [AuthorizeRole("Customer", "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSettings(UpdateUserSettingsDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await userService.UpdateUserSettingsAsync(userId, dto);

        if (result)
        {
            TempData["SuccessMessage"] = "Настройки успешно обновлены!";
        }
        else
        {
            TempData["ErrorMessage"] = "Ошибка при обновлении настроек!";
        }

        return RedirectToAction("Index");
    }
}