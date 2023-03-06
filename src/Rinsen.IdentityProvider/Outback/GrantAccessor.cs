using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rinsen.IdentityProvider.Outback.Entities;
using Rinsen.Outback.Accessors;
using Rinsen.Outback.Grants;

namespace Rinsen.IdentityProvider.Outback;


public class GrantAccessor : IGrantAccessor
{
    private readonly OutbackDbContext _outbackDbContext;

    public GrantAccessor(OutbackDbContext outbackDbContext)
    {
        _outbackDbContext = outbackDbContext;
    }

    public async Task<CodeGrant> GetCodeGrantAsync(string code)
    {
        var outbackCodeGrant = await _outbackDbContext.CodeGrants.Include(m => m.Client).SingleOrDefaultAsync(m => m.Code == code && m.Resolved == null);

        if (outbackCodeGrant == default)
        {
            throw new Exception($"Code grant not found for code {code}");
        }

        if (outbackCodeGrant.Client is null)
        {
            throw new Exception($"No client found for code {code}");
        }

        outbackCodeGrant.Resolved = DateTimeOffset.Now;
        await _outbackDbContext.SaveChangesAsync();

        return new CodeGrant
        {
            ClientId = outbackCodeGrant.Client.ClientId,
            Code = outbackCodeGrant.Code,
            CodeChallange = outbackCodeGrant.CodeChallange,
            CodeChallangeMethod = outbackCodeGrant.CodeChallangeMethod,
            Created = outbackCodeGrant.Created,
            Expires = outbackCodeGrant.Expires,
            Nonce = outbackCodeGrant.Nonce,
            RedirectUri = outbackCodeGrant.RedirectUri,
            Scope = outbackCodeGrant.Scope,
            State = outbackCodeGrant.State,
            SubjectId = outbackCodeGrant.SubjectId.ToString(),
        };
    }

    public async Task<PersistedGrant> GetPersistedGrantAsync(string clientId, string subjectId)
    {
        var subjectIdGuid = Guid.Parse(subjectId);

#pragma warning disable CS8602 // Dereference of a possibly null reference.
        var outbackPersistedGrant = await _outbackDbContext.PersistedGrants.Include(m => m.Client).SingleOrDefaultAsync(m => m.Client.ClientId == clientId && m.SubjectId == subjectIdGuid);
#pragma warning restore CS8602 // Dereference of a possibly null reference.

        if (outbackPersistedGrant == default)
        {
            throw new Exception($"Persisted grant not found for client {clientId} and subject {subjectId}");
        }

        if (outbackPersistedGrant.Client is null)
        {
            throw new Exception($"No client found for persisted grant for client {clientId} and subject {subjectId}");
        }

        return new PersistedGrant
        {
            ClientId = outbackPersistedGrant.Client.ClientId,
            Created = outbackPersistedGrant.Created,
            Expires = outbackPersistedGrant.Expires,
            Scope = outbackPersistedGrant.Scope,
            SubjectId = outbackPersistedGrant.SubjectId.ToString()
        };
    }

    public async Task<RefreshTokenGrant> GetRefreshTokenGrantAsync(string refreshToken)
    {
        var outbackPersistedGrant = await _outbackDbContext.RefreshTokenGrants.Include(m => m.Client).SingleOrDefaultAsync(m => m.RefreshToken == refreshToken && m.Resolved == null);

        if (outbackPersistedGrant == default)
        {
            throw new Exception($"Refresh token not found {refreshToken}");
        }

        if (outbackPersistedGrant.Client is null)
        {
            throw new Exception($"No client found for persisted grant for refresh token {refreshToken}");
        }

        outbackPersistedGrant.Resolved = DateTimeOffset.Now;
        await _outbackDbContext.SaveChangesAsync();

        return new RefreshTokenGrant
        {
            ClientId = outbackPersistedGrant.Client.ClientId,
            Created = outbackPersistedGrant.Created,
            Expires = outbackPersistedGrant.Expires,
            Scope = outbackPersistedGrant.Scope,
            SubjectId = outbackPersistedGrant.SubjectId.ToString(),
            RefreshToken = outbackPersistedGrant.RefreshToken,
        };
    }

    public async Task SaveCodeGrantAsync(CodeGrant codeGrant)
    {
        var clientIntId = await _outbackDbContext.Clients.Where(m => m.ClientId == codeGrant.ClientId).Select(m => m.Id).SingleAsync();

        var outbackCodeGrant = new OutbackCodeGrant
        {
            ClientId = clientIntId,
            Code = codeGrant.Code,
            CodeChallange = codeGrant.CodeChallange,
            CodeChallangeMethod = codeGrant.CodeChallangeMethod,
            Created = codeGrant.Created,
            Expires = codeGrant.Expires,
            Nonce = codeGrant.Nonce,
            RedirectUri = codeGrant.RedirectUri,
            Scope = codeGrant.Scope,
            State = codeGrant.State,
            SubjectId = Guid.Parse(codeGrant.SubjectId),
        };

        await _outbackDbContext.CodeGrants.AddAsync(outbackCodeGrant);

        await _outbackDbContext.SaveChangesAsync();
    }

    public async Task SavePersistedGrantAsync(PersistedGrant persistedGrant)
    {
        var clientIntId = await _outbackDbContext.Clients.Where(m => m.ClientId == persistedGrant.ClientId).Select(m => m.Id).SingleAsync();

        await _outbackDbContext.PersistedGrants.AddAsync(new OutbackPersistedGrant
        {
            ClientId = clientIntId,
            Expires = persistedGrant.Expires,
            Scope = persistedGrant.Scope,
            SubjectId = Guid.Parse(persistedGrant.SubjectId),
        });

        await _outbackDbContext.SaveChangesAsync();

    }

    public async Task SaveRefreshTokenGrantAsync(RefreshTokenGrant refreshTokenGrant)
    {
        var clientIntId = await _outbackDbContext.Clients.Where(m => m.ClientId == refreshTokenGrant.ClientId).Select(m => m.Id).SingleAsync();

        await _outbackDbContext.RefreshTokenGrants.AddAsync(new OutbackRefreshTokenGrant
        {
            ClientId = clientIntId,
            Expires = refreshTokenGrant.Expires,
            RefreshToken = refreshTokenGrant.RefreshToken,
            Scope = refreshTokenGrant.Scope,
            SubjectId = Guid.Parse(refreshTokenGrant.SubjectId),
        });

        await _outbackDbContext.SaveChangesAsync();
    }
}
