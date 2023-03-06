using System;
using System.Threading.Tasks;

namespace Rinsen.IdentityProvider;

public interface IIdentityStorage
{
    Task CreateAsync(Identity identity);
    Task<Identity> GetAsync(Guid identityId);
}
