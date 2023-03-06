using System;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace Rinsen.IdentityProvider.LocalAccounts;

public class LocalAccountStorage : ILocalAccountStorage
{
    private readonly IdentityOptions _identityOptions;
    
    private const string InsertSql = @"INSERT INTO LocalAccounts 
                                        (Created,
                                        FailedLoginCount,
                                        IdentityId,
                                        IsDisabled,
                                        IterationCount,
                                        LoginId,
                                        PasswordHash,
                                        PasswordSalt,
                                        Updated,
                                        SharedTotpSecret,
                                        TwoFactorTotpEnabled,
                                        TwoFactorSmsEnabled,
                                        TwoFactorEmailEnabled,
                                        TwoFactorAppNotificationEnabled,
                                        Deleted) 
                                    VALUES 
                                        (@Created,
                                        @FailedLoginCount,
                                        @IdentityId,
                                        @IsDisabled,
                                        @IterationCount,
                                        @LoginId,
                                        @PasswordHash,
                                        @PasswordSalt,
                                        @Updated,
                                        @SharedTotpSecret,
                                        @TwoFactorTotpEnabled,
                                        @TwoFactorSmsEnabled,
                                        @TwoFactorEmailEnabled,
                                        @TwoFactorAppNotificationEnabled,
                                        @Deleted); 
                                    SELECT CAST(SCOPE_IDENTITY() as int)";

    private const string SelectWithIdentityId = @"SELECT Id,
                                        IdentityId,
                                        Created,
                                        FailedLoginCount,
                                        IsDisabled,
                                        IterationCount,
                                        LoginId,
                                        PasswordHash,
                                        PasswordSalt,
                                        Updated,
                                        SharedTotpSecret,
                                        TwoFactorTotpEnabled,
                                        TwoFactorSmsEnabled,
                                        TwoFactorEmailEnabled,
                                        TwoFactorAppNotificationEnabled,
                                        Deleted 
                                    FROM 
                                        LocalAccounts 
                                    WHERE 
                                        IdentityId=@IdentityId";

    private const string SelectWithLoginId = @"SELECT Id,
                                        IdentityId,
                                        Created,
                                        FailedLoginCount,
                                        IsDisabled,
                                        IterationCount,
                                        LoginId,
                                        PasswordHash,
                                        PasswordSalt,
                                        Updated,
                                        SharedTotpSecret,
                                        TwoFactorTotpEnabled,
                                        TwoFactorSmsEnabled,
                                        TwoFactorEmailEnabled,
                                        TwoFactorAppNotificationEnabled,
                                        Deleted
                                    FROM 
                                        LocalAccounts 
                                    WHERE 
                                        LoginId=@LoginId";

    private const string UpdateFailedLoginCount = @"UPDATE 
                                                    LocalAccounts 
                                                SET 
                                                    FailedLoginCount = @FailedLoginCount,
                                                    Updated = @Updated 
                                                WHERE 
                                                    IdentityId=@IdentityId";

    private const string Update = @"UPDATE
                                    LocalAccounts
                                SET
                                    Created = @Created,
                                    FailedLoginCount = @FailedLoginCount,
                                    IdentityId = @IdentityId,
                                    IsDisabled = @IsDisabled,
                                    IterationCount = @IterationCount,
                                    LoginId = @LoginId,
                                    PasswordHash = @PasswordHash,
                                    PasswordSalt = @PasswordSalt,
                                    Updated = @Updated,
                                    SharedTotpSecret = @SharedTotpSecret,
                                    TwoFactorTotpEnabled = @TwoFactorTotpEnabled,
                                    TwoFactorSmsEnabled = @TwoFactorSmsEnabled,
                                    TwoFactorEmailEnabled = @TwoFactorEmailEnabled,
                                    TwoFactorAppNotificationEnabled = @TwoFactorAppNotificationEnabled,
                                    Deleted = @Deleted
                                WHERE 
                                    Id = @Id";


    public LocalAccountStorage(IdentityOptions identityOptions)
    {
        _identityOptions = identityOptions;
    }

    public async Task CreateAsync(LocalAccount localAccount)
    {
        localAccount.Created = DateTimeOffset.Now;
        localAccount.Updated = DateTimeOffset.Now;

        using var connection = new SqlConnection(_identityOptions.ConnectionString);

        try
        {
            using var command = new SqlCommand(InsertSql, connection);

            command.Parameters.Add(new SqlParameter("@Created", localAccount.Created));
            command.Parameters.Add(new SqlParameter("@FailedLoginCount", localAccount.FailedLoginCount));
            command.Parameters.Add(new SqlParameter("@IdentityId", localAccount.IdentityId));
            command.Parameters.Add(new SqlParameter("@IsDisabled", localAccount.IsDisabled));
            command.Parameters.Add(new SqlParameter("@IterationCount", localAccount.IterationCount));
            command.Parameters.Add(new SqlParameter("@LoginId", localAccount.LoginId));
            command.Parameters.Add(new SqlParameter("@PasswordHash", localAccount.PasswordHash));
            command.Parameters.Add(new SqlParameter("@PasswordSalt", localAccount.PasswordSalt));
            command.Parameters.Add(new SqlParameter("@Updated", localAccount.Updated));
            command.Parameters.AddWithNullableValue("@SharedTotpSecret", localAccount.SharedTotpSecret);
            command.Parameters.AddWithNullableValue("@TwoFactorAppNotificationEnabled", localAccount.TwoFactorAppNotificationEnabled);
            command.Parameters.AddWithNullableValue("@TwoFactorEmailEnabled", localAccount.TwoFactorEmailEnabled);
            command.Parameters.AddWithNullableValue("@TwoFactorSmsEnabled", localAccount.TwoFactorSmsEnabled);
            command.Parameters.AddWithNullableValue("@TwoFactorTotpEnabled", localAccount.TwoFactorTotpEnabled);
            command.Parameters.AddWithNullableValue("@Deleted", localAccount.Deleted);

            await connection.OpenAsync();

            var id = await command.ExecuteScalarAsync();

            if (id == null)
            {
                throw new Exception("Failed to create local account");
            }

            localAccount.Id = (int)id;
        }
        catch (SqlException ex)
        {
            // 2601 - Violation in unique index
            // 2627 - Violation in unique constraint
            if (ex.Number == 2601 || ex.Number == 2627)
            {
                throw new LocalAccountAlreadyExistException($"Identity {localAccount.LoginId} already exist for user {localAccount.IdentityId}", ex);
            }
            throw;
        }
    }

    public Task DeleteAsync(LocalAccount localAccount)
    {
        localAccount.Deleted = DateTimeOffset.Now;

        return UpdateAsync(localAccount);
    }

    public async Task<LocalAccount> GetAsync(string loginId)
    {
        using var connection = new SqlConnection(_identityOptions.ConnectionString);
        using var command = new SqlCommand(SelectWithLoginId, connection);

        command.Parameters.AddWithValue("@LoginId", loginId);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
            
        if (reader.HasRows && await reader.ReadAsync())
        {
            return MapLocalAccountFromReader(reader);
        }

        throw new Exception($"Could not find local account for {loginId}");
    }

    public async Task<LocalAccount> GetAsync(Guid identityId)
    {
        using var connection = new SqlConnection(_identityOptions.ConnectionString);
        using var command = new SqlCommand(SelectWithIdentityId, connection);
        
        command.Parameters.Add(new SqlParameter("@IdentityId", identityId));

        await connection.OpenAsync();

        using var reader = await command.ExecuteReaderAsync();
            
        if (reader.HasRows && await reader.ReadAsync())
        {
            return MapLocalAccountFromReader(reader);
        }

        throw new Exception($"Identity {identityId} not found");
    }

    private static LocalAccount MapLocalAccountFromReader(SqlDataReader reader)
    {
        return new LocalAccount
        {
            Created = (DateTimeOffset)reader["Created"],
            FailedLoginCount = (int)reader["FailedLoginCount"],
            IdentityId = (Guid)reader["IdentityId"],
            IsDisabled = (bool)reader["IsDisabled"],
            IterationCount = (int)reader["IterationCount"],
            Id = (int)reader["Id"],
            LoginId = (string)reader["LoginId"],
            PasswordHash = (byte[])reader["PasswordHash"],
            PasswordSalt = (byte[])reader["PasswordSalt"],
            Updated = (DateTimeOffset)reader["Updated"],
            Deleted = reader.GetValueOrDefault<DateTimeOffset?>("Deleted"),
            SharedTotpSecret = (byte[])reader["SharedTotpSecret"],
            TwoFactorAppNotificationEnabled = reader.GetValueOrDefault<DateTimeOffset?>("TwoFactorAppNotificationEnabled"),
            TwoFactorEmailEnabled = reader.GetValueOrDefault<DateTimeOffset?>("TwoFactorEmailEnabled"),
            TwoFactorSmsEnabled = reader.GetValueOrDefault<DateTimeOffset?>("TwoFactorSmsEnabled"),
            TwoFactorTotpEnabled = reader.GetValueOrDefault<DateTimeOffset?>("TwoFactorTotpEnabled")
        };
    }

    public async Task UpdateAsync(LocalAccount localAccount)
    {
        localAccount.Updated = DateTimeOffset.Now;

        using var connection = new SqlConnection(_identityOptions.ConnectionString);
        using var command = new SqlCommand(Update, connection);
        
        command.Parameters.AddWithValue("@Id", localAccount.Id);
        command.Parameters.AddWithValue("@Created", localAccount.Created);
        command.Parameters.AddWithValue("@FailedLoginCount", localAccount.FailedLoginCount);
        command.Parameters.AddWithValue("@IdentityId", localAccount.IdentityId);
        command.Parameters.AddWithValue("@IsDisabled", localAccount.IsDisabled);
        command.Parameters.AddWithValue("@IterationCount", localAccount.IterationCount);
        command.Parameters.AddWithValue("@LoginId", localAccount.LoginId);
        command.Parameters.AddWithValue("@PasswordHash", localAccount.PasswordHash);
        command.Parameters.AddWithValue("@PasswordSalt", localAccount.PasswordSalt);
        command.Parameters.AddWithValue("@Updated", localAccount.Updated);
        command.Parameters.AddWithValue("@SharedTotpSecret", localAccount.SharedTotpSecret);
        command.Parameters.AddWithNullableValue("@TwoFactorAppNotificationEnabled", localAccount.TwoFactorAppNotificationEnabled);
        command.Parameters.AddWithNullableValue("@TwoFactorEmailEnabled", localAccount.TwoFactorEmailEnabled);
        command.Parameters.AddWithNullableValue("@TwoFactorSmsEnabled", localAccount.TwoFactorSmsEnabled);
        command.Parameters.AddWithNullableValue("@TwoFactorTotpEnabled", localAccount.TwoFactorTotpEnabled);
        command.Parameters.AddWithNullableValue("@Deleted", localAccount.Deleted);
        
        await connection.OpenAsync();
        var result = await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateFailedLoginCountAsync(LocalAccount localAccount)
    {
        using var connection = new SqlConnection(_identityOptions.ConnectionString);
        using var command = new SqlCommand(UpdateFailedLoginCount, connection);

        command.Parameters.AddWithValue("@FailedLoginCount", localAccount.FailedLoginCount);
        command.Parameters.AddWithValue("@Updated", localAccount.Updated);
        command.Parameters.AddWithValue("@IdentityId", localAccount.IdentityId);
        
        await connection.OpenAsync();
        var result = await command.ExecuteNonQueryAsync();
    }
}
