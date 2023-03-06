using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rinsen.IdentityProvider.Outback.Entities;
using Rinsen.Outback.Accessors;
using Rinsen.Outback.Clients;

namespace Rinsen.IdentityProvider.Outback;

public class ClientAccessor : IClientAccessor
{
    private readonly OutbackDbContext _outbackDbContext;

    public ClientAccessor(OutbackDbContext outbackDbContext)
    {
        _outbackDbContext = outbackDbContext;
    }

    public async Task<Client> GetClient(string clientId)
    {
        var outbackClient = await _outbackDbContext.Clients
            .Include(m => m.AllowedCorsOrigins)
            .Include(m => m.ClientClaims)
            .Include(m => m.LoginRedirectUris)
            .Include(m => m.PostLogoutRedirectUris)
            .Include(m => m.Secrets)
            .Include(m => m.SupportedGrantTypes)
            .Include(m => m.Scopes).ThenInclude(m => m.Scope).SingleOrDefaultAsync(x => x.ClientId == clientId && x.Active == true);

        if (outbackClient == default)
        {
            throw new Exception($"Client {clientId} not found");
        }

        var client = new Client
        {
            AccessTokenLifetime = outbackClient.AccessTokenLifetime,
            AddUserInfoClaimsInIdentityToken = outbackClient.AddUserInfoClaimsInIdentityToken,
            AllowedCorsOrigins = outbackClient.AllowedCorsOrigins.Where(m => m.Deleted == null).Select(m => m.Origin).ToList(),
            AuthorityCodeLifetime = outbackClient.AuthorityCodeLifetime,
            ClientClaims = outbackClient.ClientClaims.Where(m => m.Deleted == null).Select(m => new ClientClaim { Type = m.Type, Value = m.Value }).ToList(),
            ClientId = outbackClient.ClientId,
            ClientType = outbackClient.ClientType,
            ConsentRequired = outbackClient.ConsentRequired,
            IdentityTokenLifetime = outbackClient.IdentityTokenLifetime,
            IssueIdentityToken = outbackClient.IssueIdentityToken,
            IssueRefreshToken = outbackClient.IssueRefreshToken,
            LoginRedirectUris = outbackClient.LoginRedirectUris.Where(m => m.Deleted == null).Select(m => m.LoginRedirectUri).ToList(),
            PostLogoutRedirectUris = outbackClient.PostLogoutRedirectUris.Where(m => m.Deleted == null).Select(m => m.PostLogoutRedirectUri).ToList(),
            RefreshTokenLifetime = outbackClient.RefreshTokenLifetime,
            SaveConsent = outbackClient.SaveConsent,
            SavedConsentLifetime = outbackClient.SavedConsentLifetime,
            Scopes = outbackClient.Scopes.Where(m => m.Deleted == null).Select(m => m.Scope.ScopeName).ToList(),
            Secrets = outbackClient.Secrets.Where(m => m.Deleted == null).Select(m => m.Secret).ToList(),
            SupportedGrantTypes = outbackClient.SupportedGrantTypes.Where(m => m.Deleted == null).Select(m => m.GrantType).ToList()
        };

        return client;
    }
}
