using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace TechStoreEll.Core.Services;

public interface ICurrentUserService
{
    int? UserId { get; }
}

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public int? UserId
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                return null;

            var possible = new[] { ClaimTypes.NameIdentifier, "sub", "id", "userId", "uid" };
            foreach (var name in possible)
            {
                var claim = user.FindFirst(name);
                if (int.TryParse(claim?.Value, out var id))
                    return id;
            }

            foreach (var claim in user.Claims)
            {
                if (int.TryParse(claim.Value, out var id))
                    return id;
            }

            return null;
        }
    }
}