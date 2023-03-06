using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Rinsen.Outback.Claims;

namespace Rinsen.IdentityProvider;

public static class AuthorizationOptionsExtensions
{
    public static void AddRequiredScopePolicy(this AuthorizationOptions authorizationOptions, string name, string scope)
    {
        authorizationOptions.AddPolicy(name, policy => policy.Requirements.Add(new RequireScope(scope)));
    }
} 

public class RequireScopeHandler : AuthorizationHandler<RequireScope>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RequireScope requirement)
    {
        if (!context.User.HasClaim(c => c.Type == StandardClaims.Scope))
        {
            return Task.CompletedTask;
        }

        var scopeValues = context.User.FindFirst(m => m.Type == StandardClaims.Scope)?.Value;

        if (string.IsNullOrEmpty(scopeValues))
        {
            return Task.CompletedTask;
        }

        if (scopeValues.Split(' ').Contains(requirement.Scope))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

public class RequireScope : IAuthorizationRequirement
{
    public RequireScope(string scope)
    {
        Scope = scope;
    }

    public string Scope { get; }
}
