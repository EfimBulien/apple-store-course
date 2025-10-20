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

        var settingsDto = new UpdateSettingsDto
        {
            Theme = user.UserSetting?.Theme ?? "light",
            ItemsPerPage = user.UserSetting?.ItemsPerPage ?? 20,
            DateFormat = user.UserSetting?.DateFormat ?? "YYYY-MM-DD",
            NumberFormat = user.UserSetting?.NumberFormat ?? "ru_RU"
        };

        var model = new ProfileViewModel
        {
            User = userDto,
            Settings = settingsDto
        };

        return View(model);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetUserSettings()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var user = await userService.GetUserWithSettingsAsync(userId);
    
        if (user?.UserSetting == null)
        {
            return Json(new {
                numberFormat = "ru_RU",
                dateFormat = "YYYY-MM-DD",
                theme = "light",
                itemsPerPage = 20
            });
        }

        return Json(new {
            numberFormat = user.UserSetting.NumberFormat ?? "ru_RU",
            dateFormat = user.UserSetting.DateFormat ?? "YYYY-MM-DD",
            theme = user.UserSetting.Theme ?? "light",
            itemsPerPage = user.UserSetting.ItemsPerPage ?? 20
        });
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
    public async Task<IActionResult> UpdateSettings(UpdateSettingsDto dto)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Неверные данные в настройках";
            return RedirectToAction("Index");
        }

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    
        var result = await userService.UpdateUserSettingsAsync(userId, new UpdateUserSettingsDto
        {
            Theme = dto.Theme,
            ItemsPerPage = dto.ItemsPerPage,
            DateFormat = dto.DateFormat,
            NumberFormat = dto.NumberFormat,
            SavedFilters = "[]",
            Hotkeys = "[]"
        });

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