using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rinsen.IdentityProvider.Outback.Entities;
using Rinsen.Outback.Accessors;
using Rinsen.Outback.Claims;

namespace Rinsen.IdentityProvider.Outback;

public class UserInfoAccessor : IUserInfoAccessor
{
    private readonly IIdentityService _identityService;
    private readonly OutbackDbContext _outbackDbContext;

    public UserInfoAccessor(IIdentityService identityService,
        OutbackDbContext outbackDbContext)
    {
        _identityService = identityService;
        _outbackDbContext = outbackDbContext;
    }
    
    public async Task<Dictionary<string, string>> GetUserInfoClaims(string subjectId, IEnumerable<string> scopeIds)
    {
        if (!Guid.TryParse(subjectId, out var identityGuid))
        {
            throw new Exception($"{subjectId} is not a valid guid");
        }

        var identity = await _identityService.GetIdentityAsync(identityGuid);
        var scopes = await _outbackDbContext.Scopes.Where(m => m.Deleted == null).Where(m => scopeIds.Contains(m.ScopeName)).Include(m => m.ScopeClaims).ToListAsync();

        var result = new Dictionary<string, string>();

        foreach (var scope in scopes)
        {
            if (scope.ClaimsInIdToken)
            {
                foreach (var claim in scope.ScopeClaims)
                {
                    switch (claim.Type)
                    {
                        case StandardClaims.Name:
                            result.Add(StandardClaims.Name, $"{identity.GivenName} {identity.Surname}");
                            break;
                        case StandardClaims.GivenName:
                            result.Add(StandardClaims.GivenName, identity.GivenName);
                            break;
                        case StandardClaims.FamilyName:
                            result.Add(StandardClaims.FamilyName, identity.Surname);
                            break;
                        case StandardClaims.Email:
                            result.Add(StandardClaims.Email, identity.Email);
                            break;
                        case StandardClaims.EmailVerified:
                            result.Add(StandardClaims.EmailVerified, identity.EmailConfirmed ? "true" : "false");
                            break;
                        case StandardClaims.PhoneNumber:
                            result.Add(StandardClaims.PhoneNumber, identity.PhoneNumber);
                            break;
                        case StandardClaims.PhoneNumberVerified:
                            result.Add(StandardClaims.PhoneNumberVerified, identity.PhoneNumberConfirmed ? "true" : "false");
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        result.Add("fredde_test", "120");

        return result;
    }
}
