using System;

namespace Rinsen.IdentityProvider.LocalAccounts;

public class LocalAccount
{
    public int Id { get; set; }

    public Guid IdentityId { get; set; }

    public string LoginId { get; set; } = string.Empty;
    
    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();

    public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();

    public byte[] SharedTotpSecret { get; set; } = Array.Empty<byte>();

    public DateTimeOffset? TwoFactorEmailEnabled { get; set; }

    public DateTimeOffset? TwoFactorSmsEnabled { get; set; }

    public DateTimeOffset? TwoFactorTotpEnabled { get; set; }

    public DateTimeOffset? TwoFactorAppNotificationEnabled { get; set; }

    public int IterationCount { get; set; }

    public int FailedLoginCount { get; set; }

    public bool IsDisabled { get; set; }

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset Updated { get; set; }

    public DateTimeOffset? Deleted { get; set; }
}
