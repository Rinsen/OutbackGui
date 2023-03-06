using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System;

namespace Rinsen.IdentityProvider;

public class IdentityAccessService : IIdentityAccessor
{
    readonly IHttpContextAccessor _httpContextAccessor;

    public IdentityAccessService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public ClaimsPrincipal ClaimsPrincipal
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null)
            {
                throw new Exception("User is null");
            }

            return user;
        }
    }

    public Guid IdentityId
    {
        get
        {
            return ClaimsPrincipal.GetClaimGuidValue(ClaimTypes.NameIdentifier);
        }
    }
}
