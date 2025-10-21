using TechStoreEll.Core.Entities;

namespace TechStoreEll.Core.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserWithSettingsAsync(int userId);
    Task<UserSetting?> GetUserSettingsAsync(int userId);
    Task<User?> GetByIdAsync(int userId);
    Task<bool> SaveChangesAsync();
    Task AddUserSettingAsync(UserSetting userSetting);
}
