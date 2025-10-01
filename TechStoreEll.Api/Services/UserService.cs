using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStoreEll.Api.DTOs;
using TechStoreEll.Api.Infrastructure.Data;
using TechStoreEll.Api.Models;

namespace TechStoreEll.Api.Services;

public class UserService(AppDbContext context)
{
    public async Task<User?> GetUserWithSettingsAsync(long userId)
    {
        return await context.Users
            .Include(u => u.UserSetting)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<bool> UpdateUserAsync(long userId, [FromForm] UpdateUserDto dto)
    {
        try
        {
            var user = await context.Users.FindAsync(userId);
            if (user == null) return false;

            user.Email = dto.Email;
            user.Phone = dto.Phone;
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.MiddleName = dto.MiddleName;
            user.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateUserSettingsAsync(long userId, [FromForm] UpdateUserSettingsDto dto)
    {
        try
        {
            var userSettings = await context.UserSettings
                .FirstOrDefaultAsync(us => us.UserId == userId);

            if (userSettings == null)
            {
                userSettings = new UserSetting
                {
                    UserId = userId,
                    Theme = dto.Theme,
                    ItemsPerPage = dto.ItemsPerPage,
                    DateFormat = dto.DateFormat,
                    NumberFormat = dto.NumberFormat,
                    SavedFilters = dto.SavedFilters,
                    Hotkeys = dto.Hotkeys,
                    UpdatedAt = DateTime.UtcNow
                };
                context.UserSettings.Add(userSettings);
            }
            else
            {
                userSettings.Theme = dto.Theme;
                userSettings.ItemsPerPage = dto.ItemsPerPage;
                userSettings.DateFormat = dto.DateFormat;
                userSettings.NumberFormat = dto.NumberFormat;
                userSettings.SavedFilters = dto.SavedFilters;
                userSettings.Hotkeys = dto.Hotkeys;
                userSettings.UpdatedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}