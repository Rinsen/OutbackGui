using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rinsen.IdentityProvider.Outback.Entities;
using Rinsen.Outback.Accessors;

namespace Rinsen.IdentityProvider.Outback;

public class AllowedCorsOriginsAccessor : IAllowedCorsOriginsAccessor
{
    private readonly OutbackDbContext _outbackDbContext;

    public AllowedCorsOriginsAccessor(OutbackDbContext outbackDbContext)
    {
        _outbackDbContext = outbackDbContext;
    }

    public async Task<HashSet<string>> GetOrigins()
    {
        var outbackAllowedCorsOrigins = await _outbackDbContext.AllowedCorsOrigins.ToListAsync();

        var result = new HashSet<string>();

        foreach (var outbackAllowedCorsOrigin in outbackAllowedCorsOrigins)
        {
            result.Add(outbackAllowedCorsOrigin.Origin);
        }

        return result;
    }
}
