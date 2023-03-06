using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rinsen.IdentityProvider;

public interface IIdentityAttributeStorage
{
    Task<IEnumerable<IdentityAttribute>> GetIdentityAttributesAsync(Guid identityId);
    Task CreateAsync(Identity identity, string v);
}
