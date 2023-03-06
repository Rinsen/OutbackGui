using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Rinsen.DatabaseInstaller;

namespace Rinsen.Outback.Gui.Installation;

public class InitializeDatabase : DatabaseVersion
{
    private readonly IConfiguration _configuration;

    public InitializeDatabase(IConfiguration configuration)
        :base(1)
    {
        _configuration = configuration;
    }

    public override void AddDbChanges(List<IDbChange> dbChangeList)
    {
        var databaseSettings = dbChangeList.AddNewDatabaseSettings();

        if (!string.IsNullOrEmpty(_configuration["Login:Debug:Password"]))
        {
            databaseSettings.CreateLogin("OutbackDebug", _configuration["Login:Debug:Password"])
            .WithUser("OutbackDebug")
            .AddRoleMembershipDataReader()
            .AddRoleMembershipDataWriter();
        }

        if (!string.IsNullOrEmpty(_configuration["Login:Runtime:Password"]))
        {
            databaseSettings.CreateLogin("OutbackRuntime", _configuration["Login:Runtime:Password"])
            .WithUser("OutbackRuntime")
            .AddRoleMembershipDataReader()
            .AddRoleMembershipDataWriter();
        }
    }
}
