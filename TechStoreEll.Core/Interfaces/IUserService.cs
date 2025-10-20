using TechStoreEll.Core.DTOs;
using TechStoreEll.Core.Entities;

namespace TechStoreEll.Core.Interfaces;

public interface IUserService
{
    Task<User?> GetUserWithSettingsAsync(int userId);
    Task<UpdateUserSettingsDto?> GetUserSettingsAsync(int userId);
    Task<bool> UpdateUserAsync(int userId, UpdateUserDto dto);
    Task<bool> UpdateUserSettingsAsync(int userId, UpdateUserSettingsDto dto);
}