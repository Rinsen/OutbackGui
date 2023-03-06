using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rinsen.IdentityProvider.Outback.Entities;
using Rinsen.Outback.Accessors;
using Rinsen.Outback.Scopes;

namespace Rinsen.IdentityProvider.Outback;

public class ScopeAccessor : IScopeAccessor
{
    private readonly OutbackDbContext _outbackDbContext;

    public ScopeAccessor(OutbackDbContext outbackDbContext)
    {
        _outbackDbContext = outbackDbContext;
    }
    
    public Task<List<Scope>> GetScopesAsync()
    {
        return _outbackDbContext.Scopes.Include(m => m.ScopeClaims).Select(s => new Scope
        {
            Audience = s.Audience,
            ScopeName = s.ScopeName,
            ShowInDiscoveryDocument = s.ShowInDiscoveryDocument,
            Claims = s.ScopeClaims.Where(m => m.Deleted == null).Select(sc => new ScopeClaim { ClaimType = sc.Type }).ToList()
        }).ToListAsync();
    }

    public Task<List<Scope>> GetScopesAsync(IReadOnlyList<string> scopes)
    {
        return _outbackDbContext.Scopes.Where(s => scopes.Contains(s.ScopeName)).Include(m => m.ScopeClaims).Select(s => new Scope
        {
            Audience= s.Audience,
            ScopeName = s.ScopeName,
            ShowInDiscoveryDocument = s.ShowInDiscoveryDocument,
            Claims = s.ScopeClaims.Where(m => m.Deleted == null).Select(sc => new ScopeClaim { ClaimType = sc.Type }).ToList()
        }).ToListAsync();
    }
}
