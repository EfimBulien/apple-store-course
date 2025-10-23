using Microsoft.EntityFrameworkCore;
using TechStoreEll.Core.DTOs;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Infrastructure.Data;

namespace TechStoreEll.Core.Services;

public class AuthService(AppDbContext context)
{
    public async Task<bool> Register(RegisterDto registerDto)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var existingUser = await context.Users.FirstOrDefaultAsync(u =>
                u.Username == registerDto.Username ||
                u.Email == registerDto.Email ||
                u.Phone == registerDto.Phone);

            if (existingUser != null)
                return false;

            var hashedPassword = HashService.HashPassword(registerDto.Password);

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

            context.UserSettings.Add(new UserSetting { Id = user.Id });
            await context.SaveChangesAsync();

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<User?> Authenticate(LoginDto loginDto)
    {
        try
        {
            var user = await context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == loginDto.Username);

            if (user == null)
                return null;

            return !HashService.VerifyHash(loginDto.Password, user.PasswordHash) ? null : user;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }
    
    public static string GenerateResetToken()
    {
        return Guid.NewGuid().ToString("N");
    }

    public async Task<bool> UpdateUserPasswordAsync(string email, string newPassword)
    {
        var user = await GetUserByEmailAsync(email);
        if (user == null)
            return false;

        user.PasswordHash = HashService.HashPassword(newPassword);
        await context.SaveChangesAsync();
        return true;
    }
}