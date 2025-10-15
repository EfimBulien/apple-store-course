using Microsoft.EntityFrameworkCore;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Infrastructure.Data;
using TechStoreEll.Core.Services.IServices;

namespace TechStoreEll.Core.Services
{
    public class UserSettingsService(AppDbContext context) : IUserSettingsService
    {
        public async Task<string> GetThemeAsync(int userId)
        {
            if (userId == 0) return "light";

            var settings = await context.UserSettings
                .FirstOrDefaultAsync(s => s.Id == userId);

            return settings?.Theme ?? "light";
        }

        public async Task<bool> UpdateThemeAsync(int userId, string theme)
        {
            if (userId == 0) return false;
            if (theme != "light" && theme != "dark") return false;

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
            return true;
        }
    }
}