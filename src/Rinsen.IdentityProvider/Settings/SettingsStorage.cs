using System;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Rinsen.IdentityProvider.Settings;

public class SettingsStorage : ISettingsStorage
{
    private readonly IConfiguration _configuration;

    public SettingsStorage(IConfiguration configuration)
    {
        _configuration = configuration;
    }
   
    public async Task CreateAsync(Setting setting)
    {
        string insertSql = @"INSERT INTO Settings (
                                    Accessed, IdentityId, KeyField, ValueField) 
                                 VALUES (
                                    @accessed, @identityId, @keyfield, @valuefield);
                                 SELECT 
                                    CAST(SCOPE_IDENTITY() as int)";

        using var connection = new SqlConnection(_configuration.GetConnectionString("InnovationBoost"));
        using var command = new SqlCommand(insertSql, connection);

        command.Parameters.Add(new SqlParameter("@accessed", setting.Accessed));
        command.Parameters.Add(new SqlParameter("@identityId", setting.IdentityId));
        command.Parameters.Add(new SqlParameter("@keyfield", setting.KeyField));
        command.Parameters.Add(new SqlParameter("@valuefield", setting.ValueField));

        await connection.OpenAsync();

        var id = await command.ExecuteScalarAsync();

        if (id == null)
        {
            throw new Exception("Failed to create setting");
        }

        setting.Id = (int)id;
    }

    public async Task UpdateAsync(Setting setting)
    {
        string insertSql = @"UPDATE Settings SET Accessed = @accessed, ValueField = @valuefield WHERE Id = @id";

        using var connection = new SqlConnection(_configuration.GetConnectionString("InnovationBoost"));
        using var command = new SqlCommand(insertSql, connection);

        command.Parameters.Add(new SqlParameter("@id", setting.Id));
        command.Parameters.Add(new SqlParameter("@accessed", setting.Accessed));
        command.Parameters.Add(new SqlParameter("@valuefield", setting.ValueField));

        await connection.OpenAsync();

        var count = await command.ExecuteNonQueryAsync();

        if (count != 1)
        {
            throw new Exception($"Failed to update id {setting.Id} with count {count}");
        }
    }

    public async Task<Setting?> GetAsync(string key, Guid identityId)
    {
        using var connection = new SqlConnection(_configuration.GetConnectionString("InnovationBoost"));
        using var command = new SqlCommand("SELECT Id, IdentityId, KeyField, ValueField, Accessed FROM Settings where KeyField = @keyfield AND IdentityId = @identityId", connection);
        
        command.Parameters.Add(new SqlParameter($"@keyfield", key));
        command.Parameters.Add(new SqlParameter($"@identityId", identityId));

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        if (reader.HasRows && await reader.ReadAsync())
        {
            return new Setting
            {
                Id = (int)reader["Id"],
                IdentityId = (Guid)reader["IdentityId"],
                KeyField = (string)reader["KeyField"],
                ValueField = (string)reader["ValueField"],
                Accessed = (DateTimeOffset)reader["Accessed"]
            };
        }

        return default;
    }
}
