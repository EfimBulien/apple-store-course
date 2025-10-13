using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStoreEll.Api.Attributes;
using TechStoreEll.Api.Entities;
using TechStoreEll.Api.Infrastructure.Data;

namespace TechStoreEll.Web.Controllers;

public class UserSettingsController(AppDbContext context) : Controller
{
    private int GetCurrentUserId()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userId ?? "0");
    }
    
    [HttpGet]
    public async Task<IActionResult> GetTheme()
    {
        var userId = GetCurrentUserId();
        var theme = "light";

        if (userId != 0)
        {
            var settings = await context.UserSettings
                .FirstOrDefaultAsync(s => s.Id == userId);
            
            theme = settings?.Theme ?? "light";
        }

        return Json(new { theme });
    }

    [AuthorizeRole("Customer", "Admin")]
    [HttpPost]
    public async Task<IActionResult> UpdateTheme(string theme)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        if (theme != "light" && theme != "dark")
        {
            return BadRequest("Invalid theme value. Use 'light' or 'dark'.");
        }

        var settings = await context.UserSettings
            .FirstOrDefaultAsync(s => s.Id == userId);

        if (settings == null)
        {
            settings = new UserSetting
            {
                Id = userId,
                Theme = theme,
                UpdatedAt = DateTime.UtcNow
            };
                
            context.UserSettings.Add(settings);
        }
        else
        {
            settings.Theme = theme;
            settings.UpdatedAt = DateTime.UtcNow;
            context.UserSettings.Update(settings);
        }

        await context.SaveChangesAsync();
        return Ok();
    }
}