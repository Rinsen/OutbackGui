using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace Rinsen.IdentityProvider;

public class IdentityAttributeStorage : IIdentityAttributeStorage
{
    private readonly IdentityOptions _identityOptions;

    private const string GetSql = @"SELECT 
                                            Attribute,
                                            Id,
                                            IdentityId,
                                            Created,
                                            Updated,
                                            Deleted
                                        FROM 
                                            IdentityAttributes 
                                        WHERE 
                                            IdentityId=@IdentityId AND Deleted is null";

    private const string _insertSql = @"INSERT INTO IdentityAttributes 
                                        (Attribute,
                                        IdentityId,
                                        Created,
                                        Updated,
                                        Deleted) 
                                    VALUES 
                                        (@Attribute,
                                        @IdentityId,
                                        @Created,
                                        @Updated,
                                        @Deleted) 
                                    SELECT CAST(SCOPE_IDENTITY() as int)";

    public IdentityAttributeStorage(IdentityOptions identityOptions)
    {
        _identityOptions = identityOptions;
    }

    public async Task<IEnumerable<IdentityAttribute>> GetIdentityAttributesAsync(Guid identityId)
    {
        var result = new List<IdentityAttribute>();

        using var connection = new SqlConnection(_identityOptions.ConnectionString);
        using var command = new SqlCommand(GetSql, connection);

        command.Parameters.Add(new SqlParameter("@IdentityId", identityId));

        await connection.OpenAsync();
        using (var reader = await command.ExecuteReaderAsync())
        {
            if (reader.HasRows)
            {
                while (await reader.ReadAsync())
                {
                    result.Add(new IdentityAttribute
                    {
                        Attribute = (string)reader["Attribute"],
                        Id = (int)reader["Id"],
                        IdentityId = (Guid)reader["IdentityId"],
                        Created = (DateTimeOffset)reader["Created"],
                        Updated = (DateTimeOffset)reader["Updated"],
                        Deleted = reader.GetValueOrDefault<DateTimeOffset?>("Deleted")
                    });
                }
            }
        }

        return result;
    }

    public async Task CreateAsync(Identity identity, string attribute)
    {
        var now = DateTimeOffset.Now;

        using var connection = new SqlConnection(_identityOptions.ConnectionString);
        using var command = new SqlCommand(_insertSql, connection);

        command.Parameters.Add(new SqlParameter("@Created", now));
        command.Parameters.Add(new SqlParameter("@Updated", now));
        command.Parameters.Add(new SqlParameter("@IdentityId", identity.IdentityId));
        command.Parameters.Add(new SqlParameter("@Attribute", attribute));
        command.Parameters.AddWithNullableValue("@Deleted", (DateTimeOffset?)null);

        await connection.OpenAsync();
        await command.ExecuteScalarAsync();
    }
}
