using System;
using System.Threading.Tasks;

namespace Rinsen.IdentityProvider.LocalAccounts
{
    public interface ILocalAccountStorage
    {
        Task CreateAsync(LocalAccount localAccount);
        Task<LocalAccount> GetAsync(Guid identityId);
        Task<LocalAccount> GetAsync(string loginId);
        Task UpdateAsync(LocalAccount localAccount);
        Task DeleteAsync(LocalAccount localAccount);
        Task UpdateFailedLoginCountAsync(LocalAccount localAccount);
    }
}
