using Microsoft.EntityFrameworkCore;
using TechStoreEll.Core.DTOs;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Infrastructure.Data;

namespace TechStoreEll.Core.Services;

public class UserService(AppDbContext context)
{
    public async Task<User?> GetUserWithSettingsAsync(int userId)
    {
        return await context.Users
            .Include(u => u.UserSetting)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<bool> UpdateUserAsync(int userId, UpdateUserDto dto)
    {
        try
        {
            var user = await context.Users.FindAsync(userId);
            if (user == null) return false;

            // обновляем только нужные поля
            user.Email = dto.Email;
            user.Phone = dto.Phone;
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.MiddleName = dto.MiddleName;
            user.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обновлении пользователя: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateUserSettingsAsync(int userId, UpdateUserSettingsDto dto)
    {
        try
        {
            var userSettings = await context.UserSettings
                .FirstOrDefaultAsync(us => us.Id == userId);

            if (userSettings == null)
            {
                userSettings = new UserSetting
                {
                    Id = userId,
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
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обновлении настроек пользователя: {ex.Message}");
            return false;
        }
    }
}