using System;
using System.Collections.Generic;
using System.Text;

namespace Rinsen.IdentityProvider.LocalAccounts;

public class TwoFactorAuthenticationSession
{
    public int Id { get; set; }

    public Guid IdentityId { get; set; }

    public string SessionId { get; set; } = string.Empty;

    public TwoFactorType Type { get; set; }

    public string KeyCode { get; set; } = string.Empty;

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset Updated { get; set; }

    public DateTimeOffset? Deleted { get; set; }

}

public enum TwoFactorType
{
    NotSelected,
    Totp,
    Sms,
    Email,
    Notification
}


