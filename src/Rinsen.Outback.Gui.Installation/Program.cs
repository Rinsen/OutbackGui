using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Rinsen.DatabaseInstaller;

namespace Rinsen.Outback.Gui.Installation;

class Program
{
    static Task Main(string[] args)
    {
        return InstallerHost.Start<InstallerStartup>();
    }
}

public class InstallerStartup : IInstallerStartup
{
    public void DatabaseVersionsToInstall(List<DatabaseVersion> databaseVersions, IConfiguration configuration)
    {
        databaseVersions.Add(new InitializeDatabase(configuration));
        databaseVersions.Add(new CreateTables());
        databaseVersions.Add(new OutbackTableInstallation());
    }
}
