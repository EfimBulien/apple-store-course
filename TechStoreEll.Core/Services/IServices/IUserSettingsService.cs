namespace TechStoreEll.Core.Services.IServices;

public interface IUserSettingsService
{
    Task<string> GetThemeAsync(int userId);
    Task<bool> UpdateThemeAsync(int userId, string theme);
}