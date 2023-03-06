using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace Rinsen.IdentityProvider;

public class SessionStorage : ISessionStorage
{
    private string _connectionString;

    private const string InsertSql = @"INSERT INTO Sessions (
                                                SessionId,
                                                IdentityId,
                                                CorrelationId,
                                                IpAddress,
                                                UserAgent,
                                                Created,
                                                Updated,
                                                Deleted,
                                                Expires,
                                                SerializedTicket) 
                                            VALUES (
                                                @SessionId,
                                                @IdentityId,
                                                @CorrelationId,
                                                @IpAddress,
                                                @UserAgent,
                                                @Created,
                                                @Updated,
                                                @Deleted,
                                                @Expires,
                                                @SerializedTicket); 
                                            SELECT CAST(SCOPE_IDENTITY() as int)";

    private const string GetSqlWithoutDeletedForIdentity = @"SELECT 
                                            Id,
                                            SessionId,
                                            IdentityId,
                                            CorrelationId,
                                            IpAddress,
                                            UserAgent,
                                            Created,
                                            Updated,
                                            Deleted,
                                            Expires,
                                            SerializedTicket
                                        FROM 
                                            Sessions 
                                        WHERE 
                                            IdentityId=@IdentityId
                                            AND Deleted is null";

    private const string GetSqlWithDeletedForIdentity = @"SELECT 
                                            Id,
                                            SessionId,
                                            IdentityId,
                                            CorrelationId,
                                            IpAddress,
                                            UserAgent,
                                            Created,
                                            Updated,
                                            Deleted,
                                            Expires,
                                            SerializedTicket
                                        FROM 
                                            Sessions 
                                        WHERE 
                                            IdentityId=@IdentityId";

    private const string GetSqlWithoutDeleted = @"SELECT 
                                            Id,
                                            SessionId,
                                            IdentityId,
                                            CorrelationId,
                                            IpAddress,
                                            UserAgent,
                                            Created,
                                            Updated,
                                            Deleted,
                                            Expires,
                                            SerializedTicket
                                        FROM 
                                            Sessions 
                                        WHERE 
                                            SessionId=@SessionId
                                            AND Deleted is null";

    private const string GetSqlWithDeleted = @"SELECT 
                                            Id,
                                            SessionId,
                                            IdentityId,
                                            CorrelationId,
                                            IpAddress,
                                            UserAgent,
                                            Created,
                                            Updated,
                                            Deleted,
                                            Expires,
                                            SerializedTicket
                                        FROM 
                                            Sessions 
                                        WHERE 
                                            SessionId=@SessionId";

    private const string DeleteSql = @"UPDATE Sessions SET Deleted = @Deleted WHERE SessionId = @SessionId";

    private const string UpdateSql = @"UPDATE Sessions SET
                                                Updated = @Updated,
                                                Expires = @Expires,
                                                Deleted = @Deleted,
                                                SerializedTicket = @SerializedTicket
                                            WHERE
                                                SessionId=@SessionId";

    public SessionStorage(string connectionString)
    {
        _connectionString = connectionString;
    }

    public SessionStorage(IdentityOptions identityOptions)
    {
        _connectionString = identityOptions.ConnectionString;
    }

    public async Task CreateAsync(Session session)
    {
        var existingSession = await GetAsync(session.SessionId, true);

        if (existingSession is not null)
        {
            if (existingSession.IdentityId == session.IdentityId && existingSession.Deleted is not null)
            {
                await UpdateAsync(session);
            }
            else
            {
                throw new SessionAlreadyExistException($"Session {session.SessionId} already exist for user {existingSession.IdentityId} while trying to create for user {session.IdentityId}");
            }
        }
        else
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var command = new SqlCommand(InsertSql, connection);

                command.Parameters.AddWithValue("@SessionId", session.SessionId);
                command.Parameters.AddWithValue("@IdentityId", session.IdentityId);
                command.Parameters.AddWithValue("@CorrelationId", session.CorrelationId);
                command.Parameters.AddWithValue("@IpAddress", session.IpAddress);
                command.Parameters.AddWithValue("@UserAgent", session.UserAgent);
                command.Parameters.AddWithValue("@Created", session.Created);
                command.Parameters.AddWithValue("@Updated", session.Updated);
                command.Parameters.AddWithValue("@Expires", session.Expires);
                command.Parameters.AddWithValue("@SerializedTicket", session.SerializedTicket);
                command.Parameters.AddWithNullableValue("@Deleted", session.Deleted);

                await connection.OpenAsync();

                var id = await command.ExecuteScalarAsync();

                if (id == null)
                {
                    throw new Exception("Failed to create session");
                }

                session.Id = (int)id;
            }

            catch (SqlException ex)
            {
                // 2601 - Violation in unique index
                // 2627 - Violation in unique constraint
                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    throw new SessionAlreadyExistException($"Session {session.SessionId} already exist while trying to create for user {session.IdentityId}", ex);
                }
                throw;
            }
        }
    }

    public async Task DeleteAsync(string sessionId)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(DeleteSql, connection);

        command.Parameters.Add(new SqlParameter("@SessionId", sessionId));
        command.Parameters.Add(new SqlParameter("@Deleted", DateTimeOffset.Now));

        await connection.OpenAsync();
        var count = await command.ExecuteNonQueryAsync();
    }

    public async Task<Session?> GetAsync(string sessionId, bool includeDeleted = false)
    {
        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand(string.Empty, connection))
        {
            if (includeDeleted)
            {
                command.CommandText = GetSqlWithDeleted;
            }
            else
            {
                command.CommandText = GetSqlWithoutDeleted;
            }

            command.Parameters.Add(new SqlParameter("@SessionId", sessionId));

            await connection.OpenAsync();

            var reader = await command.ExecuteReaderAsync();

            if (reader.HasRows && await reader.ReadAsync())
            {
                return MapSession(reader);
            }
        }

        return default;
        //throw new Exception($"Session {sessionId} not found");
    }

    public async Task<IEnumerable<Session>> GetAsync(Guid identityId, bool includeDeleted = false)
    {
        var result = new List<Session>();

        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand(string.Empty, connection))
        {
            if (includeDeleted)
            {
                command.CommandText = GetSqlWithDeletedForIdentity;
            }
            else
            {
                command.CommandText = GetSqlWithoutDeletedForIdentity;
            }

            command.Parameters.Add(new SqlParameter("@IdentityId", identityId));

            await connection.OpenAsync();

            var reader = await command.ExecuteReaderAsync();

            if (reader.HasRows)
            {
                while (await reader.ReadAsync())
                {
                    result.Add(MapSession(reader));
                }
            }
        }

        return result;
    }

    private static Session MapSession(SqlDataReader reader)
    {
        return new Session
        {
            Id = (int)reader["Id"],
            SessionId = (string)reader["SessionId"],
            IdentityId = (Guid)reader["IdentityId"],
            CorrelationId = (Guid)reader["CorrelationId"],
            IpAddress = (string)reader["IpAddress"],
            UserAgent = (string)reader["UserAgent"],
            Created = (DateTimeOffset)reader["Created"],
            Updated = (DateTimeOffset)reader["Updated"],
            Deleted = reader.GetValueOrDefault<DateTimeOffset?>("Deleted"),
            Expires = (DateTimeOffset)reader["Expires"],
            SerializedTicket = (byte[])reader["SerializedTicket"]
        };
    }

    public async Task UpdateAsync(Session session)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(UpdateSql, connection);

        command.Parameters.AddWithValue("@SessionId", session.SessionId);
        command.Parameters.AddWithValue("@Updated", session.Updated);
        command.Parameters.AddWithValue("@Expires", session.Expires);
        command.Parameters.AddWithValue("@SerializedTicket", session.SerializedTicket);
        command.Parameters.AddWithNullableValue("@Deleted", session.Deleted);

        await connection.OpenAsync();

        await command.ExecuteNonQueryAsync();
    }
}
