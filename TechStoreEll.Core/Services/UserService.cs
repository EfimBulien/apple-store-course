using TechStoreEll.Core.DTOs;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Interfaces;

namespace TechStoreEll.Core.Services;

public class UserService(IUserRepository repository) : IUserService
{
    public async Task<User?> GetUserWithSettingsAsync(int userId)
    {
        return await repository.GetUserWithSettingsAsync(userId);
    }

    public async Task<UpdateUserSettingsDto?> GetUserSettingsAsync(int userId)
    {
        var userSetting = await repository.GetUserSettingsAsync(userId);
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
        var user = await repository.GetByIdAsync(userId);
        if (user == null) return false;

        user.Email = dto.Email;
        user.Phone = dto.Phone;
        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.MiddleName = dto.MiddleName;
        user.UpdatedAt = DateTime.UtcNow;

        return await repository.SaveChangesAsync();
    }

    public async Task<bool> UpdateUserSettingsAsync(int userId, UpdateUserSettingsDto dto)
    {
        var userSetting = await repository.GetUserSettingsAsync(userId);

        if (userSetting == null)
        {
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

            await repository.AddUserSettingAsync(userSetting);
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

        return await repository.SaveChangesAsync();
    }
}
