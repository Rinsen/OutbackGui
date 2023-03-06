using System;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace Rinsen.IdentityProvider.LocalAccounts;

public class UsedTotpLogStorage : IUsedTotpLogStorage
{
    private readonly IdentityOptions _identityOptions;

    const string _insertSql = @"INSERT INTO UsedTotpLogs 
                                        (IdentityId,
                                        Code,
                                        UsedTime) 
                                    VALUES 
                                        (@IdentityId,
                                        @Code,
                                        @UsedTime); 
                                    SELECT CAST(SCOPE_IDENTITY() as int)";

    public UsedTotpLogStorage(IdentityOptions identityOptions)
    {
        _identityOptions = identityOptions;
    }

    public async Task CreateLog(UsedTotpLog usedTotpLog)
    {
        using var connection = new SqlConnection(_identityOptions.ConnectionString);
        using var command = new SqlCommand(_insertSql, connection);

        command.Parameters.Add(new SqlParameter("@IdentityId", usedTotpLog.IdentityId));
        command.Parameters.Add(new SqlParameter("@Code", usedTotpLog.Code));
        command.Parameters.Add(new SqlParameter("@UsedTime", usedTotpLog.UsedTime));

        await connection.OpenAsync();
        try
        {
            var id = await command.ExecuteScalarAsync();

            if (id == null)
            {
                throw new Exception("Failed to create totp log post");
            }

            usedTotpLog.Id = (int)id;
        }
        catch (SqlException ex)
        {
            // 2601 - Violation in unique index
            // 2627 - Violation in unique constraint
            if (ex.Number == 2601 || ex.Number == 2627)
            {
                throw new TotpCodeAlreadyUsedException($"Totp Code '{usedTotpLog.Code}' is already used for identity '{usedTotpLog.IdentityId}'", ex);
            }

            throw;
        }
    }
}
