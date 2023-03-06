using System;
using System.Security.Claims;

namespace Rinsen.IdentityProvider;

public interface IIdentityAccessor
{
    ClaimsPrincipal ClaimsPrincipal { get; }
    Guid IdentityId { get; }
}
