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
    
    public async Task<UpdateUserSettingsDto?> GetUserSettingsAsync(int userId)
    {
        var userSetting = await context.UserSettings
            .FirstOrDefaultAsync(us => us.Id == userId);
    
        if (userSetting == null)
            return null;

        return new UpdateUserSettingsDto
        {
            Theme = userSetting.Theme,
            ItemsPerPage = userSetting.ItemsPerPage,
            DateFormat = userSetting.DateFormat,
            NumberFormat = userSetting.NumberFormat,
            SavedFilters = userSetting.SavedFilters,
            Hotkeys = userSetting.Hotkeys
        };
    }

    public async Task<bool> UpdateUserAsync(int userId, UpdateUserDto dto)
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
            Console.WriteLine($"Обновление настроек для пользователя {userId}:");
            Console.WriteLine($"Theme: {dto.Theme}, ItemsPerPage: {dto.ItemsPerPage}, DateFormat: {dto.DateFormat}, NumberFormat: {dto.NumberFormat}");

            var userSetting = await context.UserSettings
                .FirstOrDefaultAsync(us => us.Id == userId);

            if (userSetting == null)
            {
                Console.WriteLine("Создание новых настроек");
                // Создаем новые настройки
                userSetting = new UserSetting
                {
                    Id = userId,
                    Theme = dto.Theme,
                    ItemsPerPage = dto.ItemsPerPage ?? 20,
                    DateFormat = dto.DateFormat,
                    NumberFormat = dto.NumberFormat,
                    SavedFilters = dto.SavedFilters ?? "[]",
                    Hotkeys = dto.Hotkeys ?? "[]",
                    UpdatedAt = DateTime.UtcNow
                };
                context.UserSettings.Add(userSetting);
            }
            else
            {
                userSetting.Theme = dto.Theme;
                userSetting.ItemsPerPage = dto.ItemsPerPage ?? userSetting.ItemsPerPage;
                userSetting.DateFormat = dto.DateFormat;
                userSetting.NumberFormat = dto.NumberFormat;
                userSetting.SavedFilters = dto.SavedFilters ?? userSetting.SavedFilters;
                userSetting.Hotkeys = dto.Hotkeys ?? userSetting.Hotkeys;
                userSetting.UpdatedAt = DateTime.UtcNow;
            }

            var changes = await context.SaveChangesAsync();
            Console.WriteLine($"Сохранено изменений: {changes}");

            return changes > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обновлении настроек: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            return false;
        }
    }
}