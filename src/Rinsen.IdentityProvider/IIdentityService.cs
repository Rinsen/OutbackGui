using System;
using System.Threading.Tasks;

namespace Rinsen.IdentityProvider
{
    public interface IIdentityService
    {
        Task<Identity> GetIdentityAsync(Guid identityId);
        Task<Identity> GetIdentityAsync();
        void UpdateIdentityDetails(string firstName, string lastName, string email, string phoneNumber);
        Task<CreateIdentityResult> CreateAsync(string firstName, string lastName, string email, string phoneNumber);
    }
}
