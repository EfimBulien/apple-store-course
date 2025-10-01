// using Microsoft.AspNetCore.Mvc;
// using TechStoreEll.Api.Attributes;
//
// namespace TechStoreEll.Web.Controllers;
//
// public class AccountController : Controller
// {
//     [AuthorizeRole("Customer", "Admin")]
//     public IActionResult Profile()
//     {
//         return View();
//     }
//
//     [AuthorizeRole("Admin")]
//     public IActionResult AdminPanel()
//     {
//         return View();
//     }
// }
using Microsoft.AspNetCore.Mvc;
using TechStoreEll.Api.Attributes;
using TechStoreEll.Api.Services;
using TechStoreEll.Api.DTOs;
using System.Security.Claims;

namespace TechStoreEll.Web.Controllers;

public class AccountController : Controller
{
    private readonly UserService _userService;

    public AccountController(UserService userService)
    {
        _userService = userService;
    }

    [AuthorizeRole("Customer", "Admin")]
    public async Task<IActionResult> Profile()
    {
        var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var user = await _userService.GetUserWithSettingsAsync(userId);
        
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
    public async Task<IActionResult> UpdateProfile(UpdateUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View("Profile", await GetProfileViewModel(dto));
        }

        var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await _userService.UpdateUserAsync(userId, dto);

        if (result)
        {
            TempData["SuccessMessage"] = "Данные успешно обновлены!";
        }
        else
        {
            TempData["ErrorMessage"] = "Ошибка при обновлении данных!";
        }

        return RedirectToAction("Profile");
    }

    [HttpPost]
    [AuthorizeRole("Customer", "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSettings(UpdateUserSettingsDto dto)
    {
        var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await _userService.UpdateUserSettingsAsync(userId, dto);

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
    public IActionResult AdminPanel()
    {
        return View();
    }

    private async Task<ProfileViewModel> GetProfileViewModel(UpdateUserDto? userDto = null)
    {
        var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var user = await _userService.GetUserWithSettingsAsync(userId);

        if (userDto == null)
        {
            userDto = new UpdateUserDto
            {
                Email = user.Email,
                Phone = user.Phone ?? "",
                FirstName = user.FirstName,
                LastName = user.LastName,
                MiddleName = user.MiddleName
            };
        }

        var settingsDto = new UpdateUserSettingsDto
        {
            Theme = user.UserSetting?.Theme,
            ItemsPerPage = user.UserSetting?.ItemsPerPage,
            DateFormat = user.UserSetting?.DateFormat,
            NumberFormat = user.UserSetting?.NumberFormat,
            SavedFilters = user.UserSetting?.SavedFilters,
            Hotkeys = user.UserSetting?.Hotkeys
        };

        return new ProfileViewModel
        {
            User = userDto,
            Settings = settingsDto
        };
    }
}

public class ProfileViewModel
{
    public required UpdateUserDto User { get; set; }
    public required UpdateUserSettingsDto Settings { get; set; }
}