using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rinsen.IdentityProvider;

public interface ISessionStorage
{
    Task CreateAsync(Session session);
    Task DeleteAsync(string sessionId);
    Task<IEnumerable<Session>> GetAsync(Guid identityId, bool includeDeleted = false);
    Task<Session?> GetAsync(string sessionId, bool includeDeleted = false);
    Task UpdateAsync(Session session);
}
