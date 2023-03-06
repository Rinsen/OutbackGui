using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Rinsen.DatabaseInstaller;
using Rinsen.IdentityProvider;
using Rinsen.IdentityProvider.AuditLogging;
using Rinsen.IdentityProvider.LocalAccounts;
using Rinsen.IdentityProvider.Settings;
using System.Collections.Generic;

namespace Rinsen.Outback.Gui.Installation;

public class CreateTables : DatabaseVersion
{
    public CreateTables()
    : base(2)
    { }

    public override void AddDbChanges(List<IDbChange> dbChangeList)
    {
        var identitiesTable = dbChangeList.AddNewTable<Identity>("Identities").SetPrimaryKeyNonClustered();
        identitiesTable.AddAutoIncrementColumn(m => m.Id, primaryKey: false).Unique("UQ_Identities_Id").Clustered();
        identitiesTable.AddColumn(m => m.IdentityId).PrimaryKey("PK_Identities_IdentityId");
        identitiesTable.AddColumn(m => m.Email, 256).Unique("UQ_Identities_Email");
        identitiesTable.AddColumn(m => m.EmailConfirmed);
        identitiesTable.AddColumn(m => m.GivenName, 128);
        identitiesTable.AddColumn(m => m.Surname, 128);
        identitiesTable.AddColumn(m => m.PhoneNumber, 128).Unique("UQ_Identities_PhoneNumber");
        identitiesTable.AddColumn(m => m.PhoneNumberConfirmed);
        identitiesTable.AddColumn(m => m.Created);
        identitiesTable.AddColumn(m => m.Updated);
        identitiesTable.AddColumn(m => m.Deleted);

        var identityAttributesTable = dbChangeList.AddNewTable<IdentityAttribute>().SetPrimaryKeyNonClustered();
        identityAttributesTable.AddAutoIncrementColumn(m => m.Id, primaryKey: false).Unique("UQ_IdentityAttributes_Id").Clustered();
        identityAttributesTable.AddColumn(m => m.IdentityId).ForeignKey("Identities", "IdentityId").Unique("UQ_IdentityAttributes_Attribute");
        identityAttributesTable.AddColumn(m => m.Attribute, 256).Unique("UQ_IdentityAttributes_Attribute");
        identityAttributesTable.AddColumn(m => m.Created);
        identityAttributesTable.AddColumn(m => m.Updated);
        identityAttributesTable.AddColumn(m => m.Deleted);

        var localAccountsTable = dbChangeList.AddNewTable<LocalAccount>();
        localAccountsTable.AddAutoIncrementColumn(m => m.Id);
        localAccountsTable.AddColumn(m => m.IdentityId).ForeignKey("Identities", "IdentityId").Unique("UQ_LocalAccounts_IdentityId");
        localAccountsTable.AddColumn(m => m.FailedLoginCount);
        localAccountsTable.AddColumn(m => m.IsDisabled);
        localAccountsTable.AddColumn(m => m.IterationCount);
        localAccountsTable.AddColumn(m => m.LoginId, 256).Unique("UQ_LocalAccounts_LoginId");
        localAccountsTable.AddColumn(m => m.PasswordHash, 16);
        localAccountsTable.AddColumn(m => m.PasswordSalt, 16);
        localAccountsTable.AddColumn(m => m.SharedTotpSecret, length: 64);
        localAccountsTable.AddColumn(m => m.TwoFactorTotpEnabled);
        localAccountsTable.AddColumn(m => m.TwoFactorSmsEnabled);
        localAccountsTable.AddColumn(m => m.TwoFactorEmailEnabled);
        localAccountsTable.AddColumn(m => m.TwoFactorAppNotificationEnabled);
        localAccountsTable.AddColumn(m => m.Created);
        localAccountsTable.AddColumn(m => m.Updated);
        localAccountsTable.AddColumn(m => m.Deleted);

        var sessionsTable = dbChangeList.AddNewTable<Session>().SetPrimaryKeyNonClustered();
        sessionsTable.AddAutoIncrementColumn(m => m.Id, primaryKey: false).Unique("UQ_Sessions_Id").Clustered();
        sessionsTable.AddColumn(m => m.SessionId, 60).PrimaryKey("PK_Sessions_SessionId");
        sessionsTable.AddColumn(m => m.IdentityId).ForeignKey("Identities");
        sessionsTable.AddColumn(m => m.CorrelationId);
        sessionsTable.AddColumn(m => m.IpAddress, 45);
        sessionsTable.AddColumn(m => m.UserAgent, 200);
        sessionsTable.AddColumn(m => m.Expires);
        sessionsTable.AddColumn(m => m.SerializedTicket);
        sessionsTable.AddColumn(m => m.Created);
        sessionsTable.AddColumn(m => m.Updated);
        sessionsTable.AddColumn(m => m.Deleted);

        var twoFactorAuthenticationSessionTable = dbChangeList.AddNewTable<TwoFactorAuthenticationSession>();
        twoFactorAuthenticationSessionTable.AddAutoIncrementColumn(m => m.Id);
        twoFactorAuthenticationSessionTable.AddColumn(m => m.IdentityId).ForeignKey("Identities");
        twoFactorAuthenticationSessionTable.AddColumn(m => m.SessionId, length: 256);
        twoFactorAuthenticationSessionTable.AddColumn(m => m.Type);
        twoFactorAuthenticationSessionTable.AddColumn(m => m.KeyCode, 256);
        twoFactorAuthenticationSessionTable.AddColumn(m => m.Created);
        twoFactorAuthenticationSessionTable.AddColumn(m => m.Updated);
        twoFactorAuthenticationSessionTable.AddColumn(m => m.Deleted);

        var usedTotpLogTable = dbChangeList.AddNewTable<UsedTotpLog>();
        usedTotpLogTable.AddAutoIncrementColumn(m => m.Id);
        usedTotpLogTable.AddColumn(m => m.IdentityId).ForeignKey("Identities").Unique("UX_UsedTotpLogs_IdentityId");
        usedTotpLogTable.AddColumn(m => m.Code, 256).Unique("UX_UsedTotpLogs_IdentityId");
        usedTotpLogTable.AddColumn(m => m.UsedTime);

        var settingTable = dbChangeList.AddNewTable<Setting>();
        settingTable.AddAutoIncrementColumn(m => m.Id);
        settingTable.AddColumn(m => m.IdentityId).ForeignKey("Identities").Unique("UQ_Settings_IdentityId_KeyField");
        settingTable.AddColumn(m => m.KeyField, length: 256).Unique("UQ_Settings_IdentityId_KeyField");
        settingTable.AddColumn(m => m.ValueField, length: 4000);
        settingTable.AddColumn(m => m.Accessed);

        var auditLogItems = dbChangeList.AddNewTable<AuditLogItem>();
        auditLogItems.AddAutoIncrementColumn(m => m.Id);
        auditLogItems.AddColumn(m => m.EventType, 256);
        auditLogItems.AddColumn(m => m.Details, 4000);
        auditLogItems.AddColumn(m => m.IpAddress, 45);
        auditLogItems.AddColumn(m => m.Timestamp);

        var dataProtectionKeys = dbChangeList.AddNewTable<DataProtectionKey>();
        dataProtectionKeys.AddAutoIncrementColumn(m => m.Id);
        dataProtectionKeys.AddColumn(m => m.FriendlyName, 256);
        dataProtectionKeys.AddColumn(m => m.Xml, 4000);

        
    }
}


