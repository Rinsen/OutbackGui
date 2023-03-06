using System;
using System.Threading.Tasks;

namespace Rinsen.IdentityProvider.Settings;

public interface ISettingsStorage
{
    Task CreateAsync(Setting setting);
    Task<Setting?> GetAsync(string key, Guid identityId);
    Task UpdateAsync(Setting setting);
}
