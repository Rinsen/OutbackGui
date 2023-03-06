using System.Collections.Generic;
using Rinsen.DatabaseInstaller;
using Rinsen.IdentityProvider.Outback.Entities;

namespace Rinsen.Outback.Gui.Installation;

public class OutbackTableInstallation : DatabaseVersion
{
    public OutbackTableInstallation()
        : base(3)
    {
    }

    public override void AddDbChanges(List<IDbChange> dbChangeList)
    {
        var outbackScopesTable = dbChangeList.AddNewTable<OutbackScope>();
        outbackScopesTable.AddAutoIncrementColumn(m => m.Id);
        outbackScopesTable.AddColumn(m => m.DisplayName, 200);
        outbackScopesTable.AddColumn(m => m.Description, 1000);
        outbackScopesTable.AddColumn(m => m.ScopeName, 200).Unique("UX_OutbackScope_ScopeName");
        outbackScopesTable.AddColumn(m => m.Audience, 200);
        outbackScopesTable.AddColumn(m => m.Enabled);
        outbackScopesTable.AddColumn(m => m.ClaimsInIdToken);
        outbackScopesTable.AddColumn(m => m.ShowInDiscoveryDocument);
        outbackScopesTable.AddColumn(m => m.Created);
        outbackScopesTable.AddColumn(m => m.Updated);
        outbackScopesTable.AddColumn(m => m.Deleted);

        var outbackScopeClaimsTable = dbChangeList.AddNewTable<OutbackScopeClaim>();
        outbackScopeClaimsTable.AddAutoIncrementColumn(m => m.Id);
        outbackScopeClaimsTable.AddColumn(m => m.ScopeId).ForeignKey<OutbackScope>(m => m.Id);
        outbackScopeClaimsTable.AddColumn(m => m.Type, 200);
        outbackScopeClaimsTable.AddColumn(m => m.Description, 1000);
        outbackScopeClaimsTable.AddColumn(m => m.Created);
        outbackScopeClaimsTable.AddColumn(m => m.Updated);
        outbackScopeClaimsTable.AddColumn(m => m.Deleted);

        var outbackClientFamiliesTable = dbChangeList.AddNewTable<OutbackClientFamily>("OutbackClientFamilies");
        outbackClientFamiliesTable.AddAutoIncrementColumn(m => m.Id);
        outbackClientFamiliesTable.AddColumn(m => m.Name, 200);
        outbackClientFamiliesTable.AddColumn(m => m.Description, 1000);
        outbackClientFamiliesTable.AddColumn(m => m.Created);
        outbackClientFamiliesTable.AddColumn(m => m.Updated);
        outbackClientFamiliesTable.AddColumn(m => m.Deleted);

        var outbackClient = dbChangeList.AddNewTable<OutbackClient>();
        outbackClient.AddAutoIncrementColumn(m => m.Id);
        outbackClient.AddColumn(m => m.Active);
        outbackClient.AddColumn(m => m.AccessTokenLifetime);
        outbackClient.AddColumn(m => m.AuthorityCodeLifetime);
        outbackClient.AddColumn(m => m.ClientFamilyId).ForeignKey("OutbackClientFamilies", "Id");
        outbackClient.AddColumn(m => m.ClientId, 200).Unique("UX_OutbackClient_ClientId");
        outbackClient.AddColumn(m => m.ClientType);
        outbackClient.AddColumn(m => m.ConsentRequired);
        outbackClient.AddColumn(m => m.Description, 1000);
        outbackClient.AddColumn(m => m.IdentityTokenLifetime);
        outbackClient.AddColumn(m => m.IssueRefreshToken);
        outbackClient.AddColumn(m => m.IssueIdentityToken);
        outbackClient.AddColumn(m => m.Name, 200);
        outbackClient.AddColumn(m => m.RefreshTokenLifetime);
        outbackClient.AddColumn(m => m.SaveConsent);
        outbackClient.AddColumn(m => m.SavedConsentLifetime);
        outbackClient.AddColumn(m => m.AddUserInfoClaimsInIdentityToken);
        outbackClient.AddColumn(m => m.Created);
        outbackClient.AddColumn(m => m.Updated);
        outbackClient.AddColumn(m => m.Deleted);

        var outbackClientAllowedCorsOriginsTable = dbChangeList.AddNewTable<OutbackClientAllowedCorsOrigin>();
        outbackClientAllowedCorsOriginsTable.AddAutoIncrementColumn(m => m.Id);
        outbackClientAllowedCorsOriginsTable.AddColumn(m => m.ClientId).ForeignKey<OutbackClient>(m => m.Id);
        outbackClientAllowedCorsOriginsTable.AddColumn(m => m.Description, 1000);
        outbackClientAllowedCorsOriginsTable.AddColumn(m => m.Origin, 1000);
        outbackClientAllowedCorsOriginsTable.AddColumn(m => m.Created);
        outbackClientAllowedCorsOriginsTable.AddColumn(m => m.Updated);
        outbackClientAllowedCorsOriginsTable.AddColumn(m => m.Deleted);

        var outbackClientClaimsTable = dbChangeList.AddNewTable<OutbackClientClaim>();
        outbackClientClaimsTable.AddAutoIncrementColumn(m => m.Id);
        outbackClientClaimsTable.AddColumn(m => m.ClientId).ForeignKey<OutbackClient>(m => m.Id);
        outbackClientClaimsTable.AddColumn(m => m.Description, 1000);
        outbackClientClaimsTable.AddColumn(m => m.Type, 100);
        outbackClientClaimsTable.AddColumn(m => m.Value, 100);
        outbackClientClaimsTable.AddColumn(m => m.Created);
        outbackClientClaimsTable.AddColumn(m => m.Updated);
        outbackClientClaimsTable.AddColumn(m => m.Deleted);

        var outbackClientLoginRedirectUrisTable = dbChangeList.AddNewTable<OutbackClientLoginRedirectUri>();
        outbackClientLoginRedirectUrisTable.AddAutoIncrementColumn(m => m.Id);
        outbackClientLoginRedirectUrisTable.AddColumn(m => m.ClientId).ForeignKey<OutbackClient>(m => m.Id);
        outbackClientLoginRedirectUrisTable.AddColumn(m => m.Description, 1000);
        outbackClientLoginRedirectUrisTable.AddColumn(m => m.LoginRedirectUri, 500);
        outbackClientLoginRedirectUrisTable.AddColumn(m => m.Created);
        outbackClientLoginRedirectUrisTable.AddColumn(m => m.Updated);
        outbackClientLoginRedirectUrisTable.AddColumn(m => m.Deleted);

        var outbackClientPostLogoutRedirectUrisTable = dbChangeList.AddNewTable<OutbackClientPostLogoutRedirectUri>();
        outbackClientPostLogoutRedirectUrisTable.AddAutoIncrementColumn(m => m.Id);
        outbackClientPostLogoutRedirectUrisTable.AddColumn(m => m.ClientId).ForeignKey<OutbackClient>(m => m.Id);
        outbackClientPostLogoutRedirectUrisTable.AddColumn(m => m.Description, 1000);
        outbackClientPostLogoutRedirectUrisTable.AddColumn(m => m.PostLogoutRedirectUri, 500);
        outbackClientPostLogoutRedirectUrisTable.AddColumn(m => m.Created);
        outbackClientPostLogoutRedirectUrisTable.AddColumn(m => m.Updated);
        outbackClientPostLogoutRedirectUrisTable.AddColumn(m => m.Deleted);

        var outbackClientScopesTable = dbChangeList.AddNewTable<OutbackClientScope>();
        outbackClientScopesTable.AddAutoIncrementColumn(m => m.Id);
        outbackClientScopesTable.AddColumn(m => m.ClientId).ForeignKey<OutbackClient>(m => m.Id);
        outbackClientScopesTable.AddColumn(m => m.ScopeId).ForeignKey<OutbackScope>(m => m.Id);
        outbackClientScopesTable.AddColumn(m => m.Created);
        outbackClientScopesTable.AddColumn(m => m.Updated);
        outbackClientScopesTable.AddColumn(m => m.Deleted);

        var outbackClientSecretsTable = dbChangeList.AddNewTable<OutbackClientSecret>();
        outbackClientSecretsTable.AddAutoIncrementColumn(m => m.Id);
        outbackClientSecretsTable.AddColumn(m => m.ClientId).ForeignKey<OutbackClient>(m => m.Id);
        outbackClientSecretsTable.AddColumn(m => m.Description, 1000);
        outbackClientSecretsTable.AddColumn(m => m.Secret, 1000);
        outbackClientSecretsTable.AddColumn(m => m.Created);
        outbackClientSecretsTable.AddColumn(m => m.Updated);
        outbackClientSecretsTable.AddColumn(m => m.Deleted);

        var outbackClientSupportedGrantTypesTable = dbChangeList.AddNewTable<OutbackClientSupportedGrantType>();
        outbackClientSupportedGrantTypesTable.AddAutoIncrementColumn(m => m.Id);
        outbackClientSupportedGrantTypesTable.AddColumn(m => m.ClientId).ForeignKey<OutbackClient>(m => m.Id);
        outbackClientSupportedGrantTypesTable.AddColumn(m => m.GrantType, 200);
        outbackClientSupportedGrantTypesTable.AddColumn(m => m.Created);
        outbackClientSupportedGrantTypesTable.AddColumn(m => m.Updated);
        outbackClientSupportedGrantTypesTable.AddColumn(m => m.Deleted);

        var outbackCodeGrantsTable = dbChangeList.AddNewTable<OutbackCodeGrant>();
        outbackCodeGrantsTable.AddAutoIncrementColumn(m => m.Id);
        outbackCodeGrantsTable.AddColumn(m => m.ClientId).ForeignKey<OutbackClient>(m => m.Id);
        outbackCodeGrantsTable.AddColumn(m => m.Created);
        outbackCodeGrantsTable.AddColumn(m => m.Code, 200);
        outbackCodeGrantsTable.AddColumn(m => m.CodeChallange, 1000);
        outbackCodeGrantsTable.AddColumn(m => m.CodeChallangeMethod, 200);
        outbackCodeGrantsTable.AddColumn(m => m.Expires);
        outbackCodeGrantsTable.AddColumn(m => m.Nonce, 1000).Null();
        outbackCodeGrantsTable.AddColumn(m => m.RedirectUri, 1000);
        outbackCodeGrantsTable.AddColumn(m => m.Resolved);
        outbackCodeGrantsTable.AddColumn(m => m.Scope, int.MaxValue);
        outbackCodeGrantsTable.AddColumn(m => m.State, 1000);
        outbackCodeGrantsTable.AddColumn(m => m.SubjectId).ForeignKey("Identities", "IdentityId");
        
        var outbackPersistedGrantsTable = dbChangeList.AddNewTable<OutbackPersistedGrant>();
        outbackPersistedGrantsTable.AddAutoIncrementColumn(m => m.Id);
        outbackPersistedGrantsTable.AddColumn(m => m.ClientId).ForeignKey<OutbackClient>(m => m.Id);
        outbackPersistedGrantsTable.AddColumn(m => m.Created);
        outbackPersistedGrantsTable.AddColumn(m => m.Expires);
        outbackPersistedGrantsTable.AddColumn(m => m.Scope);
        outbackPersistedGrantsTable.AddColumn(m => m.SubjectId).ForeignKey("Identities", "IdentityId");

        var outbackRefreshTokenGrantsTable = dbChangeList.AddNewTable<OutbackRefreshTokenGrant>();
        outbackRefreshTokenGrantsTable.AddAutoIncrementColumn(m => m.Id);
        outbackRefreshTokenGrantsTable.AddColumn(m => m.ClientId).ForeignKey<OutbackClient>(m => m.Id);
        outbackRefreshTokenGrantsTable.AddColumn(m => m.Created);
        outbackRefreshTokenGrantsTable.AddColumn(m => m.Expires);
        outbackRefreshTokenGrantsTable.AddColumn(m => m.RefreshToken, 200);
        outbackRefreshTokenGrantsTable.AddColumn(m => m.Resolved);
        outbackRefreshTokenGrantsTable.AddColumn(m => m.Scope, 2000);
        outbackRefreshTokenGrantsTable.AddColumn(m => m.SubjectId).ForeignKey("Identities", "IdentityId");

        var outbackSecretsTable = dbChangeList.AddNewTable<OutbackSecret>();
        outbackSecretsTable.AddAutoIncrementColumn(m => m.Id);
        outbackSecretsTable.AddColumn(m => m.ActiveSigningKey);
        outbackSecretsTable.AddColumn(m => m.Created);
        outbackSecretsTable.AddColumn(m => m.Deleted);
        outbackSecretsTable.AddColumn(m => m.Expires);
        outbackSecretsTable.AddColumn(m => m.CryptographyData, int.MaxValue);
        outbackSecretsTable.AddColumn(m => m.PublicKeyCryptographyType);
        outbackSecretsTable.AddColumn(m => m.Updated);
    }
}

