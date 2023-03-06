using System.Threading.Tasks;

namespace Rinsen.IdentityProvider;

public interface ILoginService
{
    Task<LoginResult> LoginAsync(string email, string password, string host, bool rememberMe);
    Task LogoutAsync();
    Task StartTotpFlow(string authSessionId);
    Task<LoginResult> ConfirmTotpCode(string authSessionId, string keyCode, string host, bool rememberMe);
}
