using Microsoft.EntityFrameworkCore;
using TechStoreEll.Api.DTOs;
using TechStoreEll.Api.Entities;
using TechStoreEll.Core.Infrastructure.Data;

namespace TechStoreEll.Api.Services;

public class AuthService(AppDbContext context)
{
    public async Task<bool> Register(RegisterDto registerDto)
    {
        var existingUser = await context.Users
            .FirstOrDefaultAsync(u => u.Username == registerDto.Username || u.Email == registerDto.Email);

        if (existingUser != null)
            return false;
        
        var hashedPassword = PasswordHasherService.HashPassword(registerDto.Password);

        var user = new User
        {
            Username = registerDto.Username,
            Email = registerDto.Email,
            Phone = registerDto.Phone,
            PasswordHash = hashedPassword,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            MiddleName = registerDto.MiddleName,
            IsActive = true,
            RoleId = 3
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();
        
        var userId = context.Users.FirstOrDefault(u => u.Username == registerDto.Username)?.Id;
        var settings = new UserSetting
        {
            Id = (int)userId!
        };
        context.UserSettings.Add(settings);
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<User?> Authenticate(LoginDto loginDto)
    {
        var user = await context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == loginDto.Username);
        
        if (user == null)
        {
            return null;
        }
        
        return !PasswordHasherService.VerifyPassword(loginDto.Password, user.PasswordHash) ? null : user;
    }
    
}
