using System.Security.Claims;

namespace Rinsen.IdentityProvider;

public class LoginResult
{
    public ClaimsPrincipal? Principal { get; private set; }
    public bool Succeeded { get; private set; }
    public bool Failed { get { return !Succeeded; } }
    public bool TwoFactorRequired { get; private set; }
    public string TwoFactorAuthenticationSessionId { get; private set; } = string.Empty;
    public bool TwoFactorEmailEnabled { get; private set; }
    public bool TwoFactorSmsEnabled { get; private set; }
    public bool TwoFactorTotpEnabled { get; private set; }
    public bool TwoFactorAppNotificationEnabled { get; private set; }
    public string LoginId { get; private set; } = string.Empty;

    public static LoginResult RequireTwoFactor(string twoFactorAuthenticationSessionId, bool twoFactorEmailEnabled,
        bool twoFactorSmsEnabled, bool twoFactorTotpEnabled, bool twoFactorAppNotificationEnabled)
    {
        return new LoginResult
        {
            TwoFactorRequired = true,
            TwoFactorAppNotificationEnabled = twoFactorAppNotificationEnabled,
            TwoFactorAuthenticationSessionId = twoFactorAuthenticationSessionId,
            TwoFactorEmailEnabled = twoFactorEmailEnabled,
            TwoFactorSmsEnabled = twoFactorSmsEnabled,
            TwoFactorTotpEnabled = twoFactorTotpEnabled
        };
    }

    public static LoginResult Failure()
    {
        return new LoginResult() { Succeeded = false };
    }

    public static LoginResult Success(ClaimsPrincipal principal, string loginId)
    {
        return new LoginResult() { Succeeded = true, Principal = principal, LoginId = loginId };
    }
}
