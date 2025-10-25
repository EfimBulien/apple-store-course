using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TechStoreEll.Core.Entities;

namespace TechStoreEll.Core.Services;

public class JwtService(IConfiguration config) : IJwtService
{
    private readonly string _key = config["JwtSettings:SecretKey"] ?? 
                                   throw new ArgumentException("JWT Key is not configured");
    private readonly string _issuer = config["JwtSettings:Issuer"] ?? "TechStoreEllApi";
    private readonly string _audience = config["JwtSettings:Audience"] ?? "TechStoreEllUsers";
    private readonly int _expireMinutes = config.GetValue<int?>("JwtSettings:ExpiryInMinutes") ?? 60;

    public string GenerateToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
    
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.Name),
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(_expireMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public interface IJwtService { string GenerateToken(User user); }