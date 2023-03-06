using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rinsen.IdentityProvider.LocalAccounts;

public interface ITwoFactorAuthenticationSessionStorage
{
    Task Create(TwoFactorAuthenticationSession twoFactorAuthenticationSession);
    Task<TwoFactorAuthenticationSession> Get(string sessionId);
    Task Update(TwoFactorAuthenticationSession twoFactorAuthenticationSession);
    Task Delete(TwoFactorAuthenticationSession twoFactorAuthenticationSession);
}
