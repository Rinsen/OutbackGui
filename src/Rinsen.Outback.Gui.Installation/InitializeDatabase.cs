using System.Collections.Generic;
using Rinsen.DatabaseInstaller;

namespace Rinsen.Outback.Gui.Installation
{
    public class InitializeDatabase : DatabaseVersion
    {
        public InitializeDatabase()
            :base(1)
        {

        }

        public override void AddDbChanges(List<IDbChange> dbChangeList)
        {
            var databaseSettings = dbChangeList.AddNewDatabaseSettings();

            databaseSettings.CreateLogin("OutbackDebug")
                .WithUser("OutbackDebug")
                .AddRoleMembershipDataReader()
                .AddRoleMembershipDataWriter();

            databaseSettings.CreateLogin("OutbackRuntime")
                .WithUser("OutbackRuntime")
                .AddRoleMembershipDataReader()
                .AddRoleMembershipDataWriter();
        }
    }
}
