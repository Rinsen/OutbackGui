using System;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Rinsen.IdentityProvider;

public class LocalIdentityForReferenceHandler
{
    private readonly string _connectionString;

    private const string _getSql = @"SELECT 
                                            IdentityId,
                                            Created 
                                        FROM 
                                            ReferenceIdentities 
                                        WHERE 
                                            IdentityId = @IdentityId";

    private const string _insertSql = @"INSERT INTO ReferenceIdentities (
                                                IdentityId,
                                                Created) 
                                            VALUES (
                                                @IdentityId,
                                                @Created)";

    public LocalIdentityForReferenceHandler(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task CreateReferenceIdentityIfNotExisting(ClaimsPrincipal claimsPrincipal)
    {
        var identityId = claimsPrincipal.GetClaimGuidValue(ClaimTypes.NameIdentifier);

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SqlCommand(_getSql, connection))
            {
                command.Parameters.Add(new SqlParameter("@IdentityId", identityId));
                using (var reader = await command.ExecuteReaderAsync())
                { 
                    if (reader.HasRows)
                    {
                        return;
                    }
                }
            }

            using (var command = new SqlCommand(_insertSql, connection))
            {
                command.Parameters.Add(new SqlParameter("@IdentityId", identityId));
                command.Parameters.Add(new SqlParameter("@Created", DateTimeOffset.Now));

                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
