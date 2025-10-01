using Microsoft.AspNetCore.Mvc;
using TechStoreEll.Api.Attributes;
using TechStoreEll.Api.Services;
using TechStoreEll.Api.DTOs;
using System.Security.Claims;
using TechStoreEll.Web.Models;

namespace TechStoreEll.Web.Controllers;

public class AccountController(UserService userService, AuditLogService auditService) : Controller
{
    [AuthorizeRole("Customer", "Admin")]
    public async Task<IActionResult> Profile()
    {
        var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
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
        
        Console.WriteLine($"Raw Email: '{Request.Form["User.Email"]}'");
        Console.WriteLine($"Raw Phone: '{Request.Form["User.Phone"]}'");
        Console.WriteLine($"Raw FirstName: '{Request.Form["User.FirstName"]}'");
        Console.WriteLine($"Raw LastName: '{Request.Form["User.LastName"]}'");
        Console.WriteLine($"Raw MiddleName: '{Request.Form["User.MiddleName"]}'");
        
        // if (!ModelState.IsValid)
        // {
        //     Console.WriteLine("Неверная модель данных");
        //     return View("Profile", model);
        // }
        
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
            return View("Profile", model);
        }

        var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        if (userId <= 0)
            return BadRequest();

        var result = await userService.UpdateUserAsync(userId, model.User);

        TempData["SuccessMessage"] = result ? "Данные успешно обновлены!" : "Ошибка при обновлении данных!";

        return RedirectToAction("Profile");
    }

    [HttpPost]
    [AuthorizeRole("Customer", "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSettings(UpdateUserSettingsDto dto)
    {
        var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await userService.UpdateUserSettingsAsync(userId, dto);

        if (result)
        {
            TempData["SuccessMessage"] = "Настройки успешно обновлены!";
        }
        else
        {
            TempData["ErrorMessage"] = "Ошибка при обновлении настроек!";
        }

        return RedirectToAction("Profile");
    }

    [AuthorizeRole("Admin")]
    public async Task<IActionResult> AdminPanel()
    {
        var logs = await auditService.GetAuditLogsAsync(100);
        var model = new AdminPanelViewModel
        {
            AuditLogs = logs
        };
        return View(model);
    } 
}