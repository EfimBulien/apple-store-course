// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Mvc.Filters;
// using System.Security.Claims;
//
// namespace TechStoreEll.Api.Attributes;
//
// [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
// public class AuthorizeRoleAttribute(string requiredRole) : Attribute, IAuthorizationFilter
// {
//     public void OnAuthorization(AuthorizationFilterContext context)
//     {
//         var user = context.HttpContext.User;
//
//         if (user.Identity is not { IsAuthenticated: true })
//         {
//             context.Result = new UnauthorizedResult();
//             return;
//         }
//
//         var roleClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
//
//         if (roleClaim != null && roleClaim == requiredRole) return;
//         context.Result = new ForbidResult();
//     }
// }

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace TechStoreEll.Api.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class AuthorizeRoleAttribute(params string[] allowedRoles) : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (user.Identity is not { IsAuthenticated: true })
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var roleClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

        if (roleClaim != null && allowedRoles.Contains(roleClaim)) return;
        context.Result = new ForbidResult();
    }
}