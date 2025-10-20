using Microsoft.EntityFrameworkCore;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Interfaces;

namespace TechStoreEll.Core.Infrastructure.Data.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task<User?> GetUserWithSettingsAsync(int userId)
    {
        return await context.Users
            .Include(u => u.UserSetting)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<UserSetting?> GetUserSettingsAsync(int userId)
    {
        return await context.UserSettings
            .FirstOrDefaultAsync(us => us.Id == userId);
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        return await context.Users.FindAsync(userId);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }
    
    public async Task AddUserSettingAsync(UserSetting userSetting)
    {
        await context.UserSettings.AddAsync(userSetting);
    }
}