using System;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace Rinsen.IdentityProvider.AuditLogging;

public class AuditLogStorage
{
    private readonly IdentityOptions _identityOptions;

    private const string InsertSql = @"INSERT INTO AuditLogItems 
                                        (Details,
                                        EventType,
                                        IpAddress,
                                        Timestamp) 
                                    VALUES 
                                        (@Details,
                                        @EventType,
                                        @IpAddress,
                                        @Timestamp); 
                                    SELECT CAST(SCOPE_IDENTITY() as int)";

    public AuditLogStorage(IdentityOptions identityOptions)
    {
        _identityOptions = identityOptions;
    }

    internal async Task LogAsync(AuditLogItem auditLogItem)
    {
        using var connection = new SqlConnection(_identityOptions.ConnectionString);
        using var command = new SqlCommand(InsertSql, connection);

        command.Parameters.Add(new SqlParameter("@Details", auditLogItem.Details));
        command.Parameters.Add(new SqlParameter("@EventType", auditLogItem.EventType));
        command.Parameters.Add(new SqlParameter("@IpAddress", auditLogItem.IpAddress));
        command.Parameters.Add(new SqlParameter("@Timestamp", auditLogItem.Timestamp));

        await connection.OpenAsync();

        var result = await command.ExecuteScalarAsync();

        if (result == null)
            throw new Exception("Failed to create log");

        auditLogItem.Id = (int)result;
    }
}
