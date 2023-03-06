using System;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace Rinsen.IdentityProvider;

public class IdentityStorage : IIdentityStorage
{
    private readonly IdentityOptions _identityOptions;

    private const string CreateSql = @"INSERT INTO Identities (
                                                Created,
                                                Email,
                                                EmailConfirmed,
                                                GivenName,
                                                IdentityId,
                                                Surname,
                                                PhoneNumber,
                                                PhoneNumberConfirmed,
                                                Updated,
                                                Deleted) 
                                            VALUES (
                                                @Created,
                                                @Email,
                                                @EmailConfirmed,
                                                @GivenName,
                                                @IdentityId,
                                                @Surname,
                                                @PhoneNumber,
                                                @PhoneNumberConfirmed,
                                                @Updated,
                                                @Deleted); 
                                            SELECT CAST(SCOPE_IDENTITY() as int)";

    private const string _getSql = @"SELECT 
                                            Created,
                                            Email,
                                            EmailConfirmed,
                                            GivenName, 
                                            Id, 
                                            IdentityId, 
                                            Surname,
                                            PhoneNumber, 
                                            PhoneNumberConfirmed, 
                                            Updated,
                                            Deleted
                                        FROM 
                                            Identities 
                                        WHERE 
                                            IdentityId=@IdentityId";

    public IdentityStorage(IdentityOptions identityOptions)
    {
        _identityOptions = identityOptions;
    }

    public async Task CreateAsync(Identity identity)
    {
        try
        {
            using var connection = new SqlConnection(_identityOptions.ConnectionString);
            using var command = new SqlCommand(CreateSql, connection);
            
            command.Parameters.AddWithValue("@Created", identity.Created);
            command.Parameters.AddWithValue("@Email", identity.Email);
            command.Parameters.AddWithValue("@EmailConfirmed", identity.EmailConfirmed);
            command.Parameters.AddWithValue("@GivenName", identity.GivenName);
            command.Parameters.AddWithValue("@IdentityId", identity.IdentityId);
            command.Parameters.AddWithValue("@Surname", identity.Surname);
            command.Parameters.AddWithValue("@PhoneNumber", identity.PhoneNumber);
            command.Parameters.AddWithValue("@PhoneNumberConfirmed", identity.PhoneNumberConfirmed);
            command.Parameters.AddWithValue("@Updated", identity.Updated);
            command.Parameters.AddWithNullableValue("@Deleted", identity.Deleted);

            await connection.OpenAsync();

            var id = await command.ExecuteScalarAsync();

            if (id == null)
            {
                throw new Exception("Failed to create identity");
            }

            identity.Id = (int)id;
        }
        catch (SqlException ex)
        {
            if (ex.Number == 2601 || ex.Number == 2627)
            {
                throw new IdentityAlreadyExistException($"Identity {identity.IdentityId} already exist", ex);
            }

            throw;
        }
    }
    
    public async Task<Identity> GetAsync(Guid identityId)
    {
        using var connection = new SqlConnection(_identityOptions.ConnectionString);
        using var command = new SqlCommand(_getSql, connection);

        command.Parameters.AddWithValue("@IdentityId", identityId);

        await connection.OpenAsync();
        var reader = await command.ExecuteReaderAsync();

        if (reader.HasRows && await reader.ReadAsync())
        {
            return new Identity
            {
                Created = (DateTimeOffset)reader["Created"],
                Email = (string)reader["Email"],
                EmailConfirmed = (bool)reader["EmailConfirmed"],
                GivenName = (string)reader["GivenName"],
                Surname = (string)reader["Surname"],
                Id = (int)reader["Id"],
                IdentityId = (Guid)reader["IdentityId"],
                PhoneNumber = (string)reader["PhoneNumber"],
                PhoneNumberConfirmed = (bool)reader["PhoneNumberConfirmed"],
                Updated = (DateTimeOffset)reader["Updated"]
            };
        }

        throw new Exception($"Failed to get identity {identityId}");
    }
}
