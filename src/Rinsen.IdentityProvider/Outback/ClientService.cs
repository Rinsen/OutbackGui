using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rinsen.IdentityProvider.Outback.Entities;
using Rinsen.Outback.Clients;
using Rinsen.Outback.Helpers;

namespace Rinsen.IdentityProvider.Outback;

public class ClientService
{
    private readonly OutbackDbContext _outbackDbContext;

    public ClientService(OutbackDbContext outbackDbContext)
    {
        _outbackDbContext = outbackDbContext;
    }
    
    public Task<List<OutbackClient>> GetAll()
    {
        return _outbackDbContext.Clients
            .Include(m => m.AllowedCorsOrigins)
            .Include(m => m.ClientClaims)
            .Include(m => m.ClientFamily)
            .Include(m => m.LoginRedirectUris)
            .Include(m => m.PostLogoutRedirectUris)
            .Include(m => m.Scopes).ThenInclude(m => m.Scope).ThenInclude(m => m.ScopeClaims)
            .Include(m => m.Secrets)
            .Include(m => m.SupportedGrantTypes)
            .AsSingleQuery().ToListAsync();
    }

    public Task<OutbackClient?> GetClient(string clientId)
    {
        return _outbackDbContext.Clients.Where(m => m.ClientId == clientId)
            .Include(m => m.AllowedCorsOrigins)
            .Include(m => m.ClientClaims)
            .Include(m => m.ClientFamily)
            .Include(m => m.LoginRedirectUris)
            .Include(m => m.PostLogoutRedirectUris)
            .Include(m => m.Scopes).ThenInclude(m => m.Scope).ThenInclude(m => m.ScopeClaims)
            .Include(m => m.Secrets)
            .Include(m => m.SupportedGrantTypes)
            .AsSingleQuery().SingleOrDefaultAsync();
    }

    public async Task DeleteClient(string clientId)
    {
        var client = await _outbackDbContext.Clients.SingleOrDefaultAsync(m => m.ClientId == clientId);

        if (client == default)
        {
            throw new Exception($"Client with id {clientId} not found");
        }

        _outbackDbContext.Clients.Remove(client);

        await _outbackDbContext.SaveChangesAsync();
    }

    public Task<List<OutbackClientFamily>> GetAllClientFamilies()
    {
        return _outbackDbContext.ClientFamilies.ToListAsync();
    }

    public async Task CreateNewClient(string clientId, string clientName, string description, int familyId, ClientType clientType)
    {
        var outbackClient = new OutbackClient
        {
            ClientId = clientId,
            ClientType = clientType,
            Description = description,
            Name = clientName,
            ClientFamilyId = familyId
        };

        await _outbackDbContext.AddAsync(outbackClient);

        await _outbackDbContext.SaveChangesAsync();
    }

    public async Task UpdateClient(string id, OutbackClient updatedClient)
    {
        var client = await GetClient(id);

        if (client == default)
        {
            throw new Exception($"Client with id {id} not found");
        }

        if (ClientPropertiesNotEquals(client, updatedClient))
        {
            UpdateClientProperties(client, updatedClient);
        }

        UpdateAllowedCorsOrigins(client, updatedClient);
        UpdateClientClaims(client, updatedClient);
        UpdateLoginRedirectUris(client, updatedClient);
        UpdatePostLogoutRedirectUris(client, updatedClient);
        await UpdateScopesAsync(client, updatedClient);
        UpdateSecrets(client, updatedClient);
        UpdateSupportedGrantTypes(client, updatedClient);

        await _outbackDbContext.SaveChangesAsync();
    }

    private void UpdateAllowedCorsOrigins(OutbackClient client, OutbackClient updatedClient)
    {
        var existingAllowedCorsOrigins = new List<OutbackClientAllowedCorsOrigin>();

        foreach (var allowedCorsOrigin in updatedClient.AllowedCorsOrigins)
        {
            var existingAllowedCorsOrigin = client.AllowedCorsOrigins.SingleOrDefault(m => m.Origin == allowedCorsOrigin.Origin);

            if (existingAllowedCorsOrigin == default)
            {
                var newAllowedCorsOrigin = new OutbackClientAllowedCorsOrigin
                {
                    Origin = allowedCorsOrigin.Origin,
                    Description = allowedCorsOrigin.Description
                };

                client.AllowedCorsOrigins.Add(newAllowedCorsOrigin);
                existingAllowedCorsOrigins.Add(newAllowedCorsOrigin);
            }
            else
            {
                if (existingAllowedCorsOrigin.Deleted != null)
                {
                    existingAllowedCorsOrigin.Deleted = null;
                }

                if (existingAllowedCorsOrigin.Description != allowedCorsOrigin.Description)
                {
                    existingAllowedCorsOrigin.Description = allowedCorsOrigin.Description;
                }

                existingAllowedCorsOrigins.Add(existingAllowedCorsOrigin);
            }
        }

        var allowedCorsOriginsToDelete = client.AllowedCorsOrigins.Except(existingAllowedCorsOrigins).ToList();

        if (allowedCorsOriginsToDelete.Any())
        {
            _outbackDbContext.AllowedCorsOrigins.RemoveRange(allowedCorsOriginsToDelete);
        }
    }

    private void UpdateClientClaims(OutbackClient client, OutbackClient updatedClient)
    {
        var existingClientClaims = new List<OutbackClientClaim>();

        foreach (var clientClaim in updatedClient.ClientClaims)
        {
            var existingClientClaim = client.ClientClaims.SingleOrDefault(m => m.Type == clientClaim.Type);

            if (existingClientClaim == default)
            {
                var newClientClaim = new OutbackClientClaim
                {
                    Type = clientClaim.Type,
                    Description = clientClaim.Description,
                    Value = clientClaim.Value
                };

                client.ClientClaims.Add(newClientClaim);
                existingClientClaims.Add(newClientClaim);
            }
            else
            {
                if (existingClientClaim.Deleted != null)
                {
                    existingClientClaim.Deleted = null;
                }

                if (existingClientClaim.Description != clientClaim.Description ||
                existingClientClaim.Value != clientClaim.Value)
                {
                    existingClientClaim.Description = clientClaim.Description;
                    existingClientClaim.Value = clientClaim.Value;
                }

                existingClientClaims.Add(existingClientClaim);
            }
        }

        var existingClientClaimsToDelete = client.ClientClaims.Except(existingClientClaims).ToList();

        if (existingClientClaimsToDelete.Any())
        {
            _outbackDbContext.ClientClaims.RemoveRange(existingClientClaimsToDelete);
        }
    }

    private void UpdateLoginRedirectUris(OutbackClient client, OutbackClient updatedClient)
    {
        var existingLogonRedirectUris = new List<OutbackClientLoginRedirectUri>();

        foreach (var loginRedirectUri in updatedClient.LoginRedirectUris.Where(m => !string.IsNullOrEmpty(m.LoginRedirectUri)))
        {
            var existingLoginRedirectUri = client.LoginRedirectUris.SingleOrDefault(m => m.Id == loginRedirectUri.Id);

            if (existingLoginRedirectUri == default)
            {
                var newLoginRedirectUri = new OutbackClientLoginRedirectUri
                {
                    Description = loginRedirectUri.Description,
                    LoginRedirectUri = loginRedirectUri.LoginRedirectUri
                };

                client.LoginRedirectUris.Add(newLoginRedirectUri);
                existingLogonRedirectUris.Add(newLoginRedirectUri);
            }
            else
            {
                if (existingLoginRedirectUri.Deleted != null)
                {
                    existingLoginRedirectUri.Deleted = null;
                }

                if (existingLoginRedirectUri.Description != loginRedirectUri.Description ||
                    existingLoginRedirectUri.LoginRedirectUri != loginRedirectUri.LoginRedirectUri)
                {
                    existingLoginRedirectUri.Description = loginRedirectUri.Description;
                    existingLoginRedirectUri.LoginRedirectUri = loginRedirectUri.LoginRedirectUri;
                }

                existingLogonRedirectUris.Add(existingLoginRedirectUri);
            }
        }

        var existingLogonRedirectUrisToDelete = client.LoginRedirectUris.Except(existingLogonRedirectUris).ToList();

        if (existingLogonRedirectUrisToDelete.Any())
        {
            _outbackDbContext.ClientLoginRedirectUris.RemoveRange(existingLogonRedirectUrisToDelete);
        }
    }

    private void UpdatePostLogoutRedirectUris(OutbackClient client, OutbackClient updatedClient)
    {
        var existingPostLogoutRedirectUris = new List<OutbackClientPostLogoutRedirectUri>();

        foreach (var postLogoutRedirectUri in updatedClient.PostLogoutRedirectUris)
        {
            var existingPostLogoutRedirectUri = client.PostLogoutRedirectUris.SingleOrDefault(m => m.PostLogoutRedirectUri == postLogoutRedirectUri.PostLogoutRedirectUri);

            if (existingPostLogoutRedirectUri == default)
            {
                var newPostLogoutRedirectUri = new OutbackClientPostLogoutRedirectUri
                {
                    Description = postLogoutRedirectUri.Description,
                    PostLogoutRedirectUri = postLogoutRedirectUri.PostLogoutRedirectUri
                };

                client.PostLogoutRedirectUris.Add(newPostLogoutRedirectUri);
                existingPostLogoutRedirectUris.Add(newPostLogoutRedirectUri);
            }
            else
            {
                if (existingPostLogoutRedirectUri.Deleted != null)
                {
                    existingPostLogoutRedirectUri.Deleted = null;
                }

                if (existingPostLogoutRedirectUri.Description != postLogoutRedirectUri.Description ||
                    existingPostLogoutRedirectUri.PostLogoutRedirectUri != postLogoutRedirectUri.PostLogoutRedirectUri)
                {
                    existingPostLogoutRedirectUri.Description = postLogoutRedirectUri.Description;
                    existingPostLogoutRedirectUri.PostLogoutRedirectUri = postLogoutRedirectUri.PostLogoutRedirectUri;
                }

                existingPostLogoutRedirectUris.Add(existingPostLogoutRedirectUri);
            }
        }

        var existingPostLogoutRedirectUrisToDelete = client.PostLogoutRedirectUris.Except(existingPostLogoutRedirectUris).ToList();

        if (existingPostLogoutRedirectUrisToDelete.Any())
        {
            _outbackDbContext.ClientPostLogoutRedirectUris.RemoveRange(existingPostLogoutRedirectUrisToDelete);
        }
    }

    private async Task UpdateScopesAsync(OutbackClient client, OutbackClient updatedClient)
    {
        var existingScopes = new List<OutbackClientScope>();

        foreach (var scope in updatedClient.Scopes)
        {
            var existingScope = client.Scopes.SingleOrDefault(m => m.ScopeId == scope.ScopeId);

            if (existingScope == default)
            {
                var dbScope = await _outbackDbContext.Scopes.SingleOrDefaultAsync(m => m.Id == scope.ScopeId);

                if (dbScope == default)
                {
                    continue;
                }

                var newScope = new OutbackClientScope
                {
                    ScopeId = scope.ScopeId,
                    Scope = dbScope
                };

                client.Scopes.Add(newScope);
                existingScopes.Add(newScope);
            }
            else
            {
                if (existingScope.Deleted != null)
                {
                    existingScope.Deleted = null;
                }

                existingScopes.Add(existingScope);
            }
        }

        var existingScopesToDelete = client.Scopes.Except(existingScopes).ToList();

        if (existingScopesToDelete.Any())
        {
            _outbackDbContext.ClientScopes.RemoveRange(existingScopesToDelete);
        }
    }

    private void UpdateSecrets(OutbackClient client, OutbackClient updatedClient)
    {
        var existingScopes = new List<OutbackClientSecret>();

        foreach (var secret in updatedClient.Secrets)
        {
            var existingSecret = client.Secrets.SingleOrDefault(m => m.Id == secret.Id);

            if (existingSecret == default)
            {
                var newSecret = new OutbackClientSecret
                {
                    Description = secret.Description,
                    Secret = HashHelper.GetSha256Hash(secret.Secret)
                };

                client.Secrets.Add(newSecret);
                existingScopes.Add(newSecret);
            }
            else
            {
                if (existingSecret.Deleted != null)
                {
                    existingSecret.Deleted = null;
                }

                if (existingSecret.Description != secret.Description)
                {
                    existingSecret.Description = secret.Description;
                }

                existingScopes.Add(existingSecret);
            }
        }

        var existingSecretsToDelete = client.Secrets.Except(existingScopes).ToList();

        if (existingSecretsToDelete.Any())
        {
            _outbackDbContext.ClientSecrets.RemoveRange(existingSecretsToDelete);
        }
    }

    private void UpdateSupportedGrantTypes(OutbackClient client, OutbackClient updatedClient)
    {
        var existingGrantTypes = new List<OutbackClientSupportedGrantType>();

        foreach (var grantType in updatedClient.SupportedGrantTypes)
        {
            var existingGrantType = client.SupportedGrantTypes.SingleOrDefault(m => m.GrantType == grantType.GrantType);

            if (existingGrantType == default)
            {
                var newGrant = new OutbackClientSupportedGrantType
                {
                    GrantType = grantType.GrantType
                };

                client.SupportedGrantTypes.Add(newGrant);
                existingGrantTypes.Add(newGrant);
            }
            else
            {
                if (existingGrantType.Deleted != null)
                {
                    existingGrantType.Deleted = null;
                }

                existingGrantTypes.Add(existingGrantType);
            }
        }

        var supportedGrantTypeToDelete = client.SupportedGrantTypes.Except(existingGrantTypes).ToList();

        if (supportedGrantTypeToDelete.Any())
        {
            _outbackDbContext.ClientSupportedGrantTypes.RemoveRange(supportedGrantTypeToDelete);
        }
    }


    public async Task<OutbackClientFamily> CreateNewFamily(string name, string description)
    {
        var outbackClientFamily = new OutbackClientFamily
        {
            Description = description,
            Name = name,
        };

        await _outbackDbContext.AddAsync(outbackClientFamily);

        await _outbackDbContext.SaveChangesAsync();

        return outbackClientFamily;
    }

    private static bool ClientPropertiesNotEquals(OutbackClient client, OutbackClient updatedClient)
    {
        return client.AccessTokenLifetime != updatedClient.AccessTokenLifetime
            || client.Active != updatedClient.Active
            || client.AddUserInfoClaimsInIdentityToken != updatedClient.AddUserInfoClaimsInIdentityToken
            || client.AuthorityCodeLifetime != updatedClient.AuthorityCodeLifetime
            || client.ClientFamilyId != updatedClient.ClientFamilyId
            || client.ClientType != updatedClient.ClientType
            || client.ConsentRequired != updatedClient.ConsentRequired
            || client.Description != updatedClient.Description
            || client.IdentityTokenLifetime != updatedClient.IdentityTokenLifetime
            || client.IssueRefreshToken != updatedClient.IssueRefreshToken
            || client.Name != updatedClient.Name
            || client.SaveConsent != updatedClient.SaveConsent
            || client.SavedConsentLifetime != updatedClient.SavedConsentLifetime;
    }

    private static void UpdateClientProperties(OutbackClient client, OutbackClient updatedClient)
    {
        client.AccessTokenLifetime = updatedClient.AccessTokenLifetime;
        client.Active = updatedClient.Active;
        client.AddUserInfoClaimsInIdentityToken = updatedClient.AddUserInfoClaimsInIdentityToken;
        client.AuthorityCodeLifetime = updatedClient.AuthorityCodeLifetime;
        client.ClientFamilyId = updatedClient.ClientFamilyId;
        client.ClientType = updatedClient.ClientType;
        client.ConsentRequired = updatedClient.ConsentRequired;
        client.Description = updatedClient.Description;
        client.IdentityTokenLifetime = updatedClient.IdentityTokenLifetime;
        client.IssueRefreshToken = updatedClient.IssueRefreshToken;
        client.Name = updatedClient.Name;
        client.SaveConsent = updatedClient.SaveConsent;
        client.SavedConsentLifetime = updatedClient.SavedConsentLifetime;
    }
}
